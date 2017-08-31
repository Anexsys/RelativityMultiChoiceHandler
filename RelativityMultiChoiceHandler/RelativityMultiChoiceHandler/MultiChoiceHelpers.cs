using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelativityMultiChoiceHandler
{
    public class MultiChoiceHelpers
    {
        /// <summary>
        /// Gets a list of Choices in a MultiChoiceFieldValueList from a list of strings, and creates any choices that don't exist in Relativity
        /// </summary>
        /// <param name="db">The IDBContext for the workspace</param>
        /// <param name="proxy">The IRSAPIClient for the workspace</param>
        /// <param name="fieldName">The name of the Multi Choice field to be updated</param>
        /// <param name="choiceNames">A list of choices that will be created/added</param>
        /// <returns>A MultiChoiceFieldValueList that can be assigned to a FieldValue for a Multi Choice field</returns>
        public static MultiChoiceFieldValueList CreateAndGetMultiChoices(IDBContext db, IRSAPIClient proxy, string fieldName, List<string> choiceNames)
        {
            if (choiceNames.Count == 0) { return new MultiChoiceFieldValueList(); }

            var choiceList = new MultiChoiceFieldValueList();

            try
            {
                foreach (var choice in choiceNames)
                {
                    //does it contain separators
                    if (choice.Contains(@"/"))
                    {
                        kCura.Relativity.Client.DTOs.Artifact currentParent = null;
                        foreach (var choiceName in choice.Split('/'))
                        {
                            //create the choice and store the parent
                            currentParent = GetMultiChoice(db, proxy, fieldName, choiceName, currentParent);
                            if (currentParent != null)
                            {
                                choiceList.Add((kCura.Relativity.Client.DTOs.Choice)currentParent);
                            }
                        }
                    }
                    else
                    {
                        choiceList.Add(GetMultiChoice(db, proxy, fieldName, choice));
                    }
                }
            }
            catch { return new MultiChoiceFieldValueList(); }

            return choiceList;

        }

        #region Private Methods

        private static kCura.Relativity.Client.DTOs.Choice GetMultiChoice(IDBContext db, IRSAPIClient proxy, string fieldName, string value, kCura.Relativity.Client.DTOs.Artifact parentArtifact = null)
        {
            try
            {
                var codeTypeId = GetCodeTypeOfField(db, fieldName);
                var choice = GetChoice(proxy, db, codeTypeId, value, parentArtifact);
                if (choice != null)
                {
                    return choice;
                }

                var choiceIds = GetChoiceId(db, codeTypeId, value);

                try
                {
                    return proxy.Repositories.Choice.ReadSingle(choiceIds);
                }
                catch
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get Multi Choice Field Value List. ", ex);
            }
        }

        private static List<int> GetChoiceIds(IDBContext db, int codeTypeId, List<string> values)
        {
            var choiceIds = new List<int>();
            foreach (var h in values)
            {
                var value = h.Trim();
                if (value.Length < 1) { continue; }

                var id = GetChoiceId(db, codeTypeId, value);
                if (id > 0 && !choiceIds.Contains(id))
                {
                    choiceIds.Add(id);
                }
            }
            return choiceIds;
        }

        private static List<kCura.Relativity.Client.DTOs.Choice> GetChoices(DBContext db, int codeTypeId, List<string> values)
        {
            var choices = new List<kCura.Relativity.Client.DTOs.Choice>();

            if (codeTypeId < 0) { return choices; }

            choices.AddRange(values.Select(h => h.Trim())
                .Where(value => value.Length >= 1)
                .Where(value => !ChoiceExists(db, codeTypeId, value))
                .Select(value => new kCura.Relativity.Client.DTOs.Choice()
                {
                    ChoiceTypeID = codeTypeId,
                    Name = value,
                    Order = 10,
                    HighlightStyleID = (int)HighlightColor.Green
                }));

            return choices;
        }

        private static kCura.Relativity.Client.DTOs.Choice GetChoice(IRSAPIClient proxy, IDBContext db, int codeTypeId, string value, kCura.Relativity.Client.DTOs.Artifact parentArtifact)
        {
            //if the choice does not exist in Relativity, create it

            if (codeTypeId < 0) { return null; }

            value = value.Trim();

            if (ChoiceExists(db, codeTypeId, value)) return null;
            if (parentArtifact != null)
            {

                var choice = new kCura.Relativity.Client.DTOs.Choice()
                {
                    ChoiceTypeID = codeTypeId,
                    Name = value,
                    Order = 10,
                    HighlightStyleID = (int)HighlightColor.Green,
                    ParentArtifact = parentArtifact
                };

                return CreateChoiceInRelativity(proxy, choice);
            }
            else
            {
                var choice = new kCura.Relativity.Client.DTOs.Choice()
                {
                    ChoiceTypeID = codeTypeId,
                    Name = value,
                    Order = 10,
                    HighlightStyleID = (int)HighlightColor.Green
                };

                return CreateChoiceInRelativity(proxy, choice);
            }
        }

        private static kCura.Relativity.Client.DTOs.Choice CreateChoiceInRelativity(IRSAPIClient proxy, kCura.Relativity.Client.DTOs.Choice choice)
        {
            WriteResultSet<kCura.Relativity.Client.DTOs.Choice> writeResult = null;
            try
            {
                writeResult = proxy.Repositories.Choice.Create(choice);
                return writeResult.Success ? writeResult.Results[0].Artifact : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #region Sql Queries

        private static bool ChoiceExists(IDBContext db, int codeTypeId, string choiceName)
        {
            var sql =
                @"SELECT TOP 1 [ArtifactID] FROM [EDDSDBO].[Code] (nolock) 
                    WHERE [Name]='" + choiceName + "' AND [CodeTypeId]=" + codeTypeId;

            try { return (db.ExecuteSqlStatementAsScalar<int>(sql) > 0) ? true : false; }
            catch { return false; }
        }

        private static int GetChoiceId(IDBContext db, int codeTypeId, string choiceName)
        {
            var sql =
                @"SELECT TOP 1 [ArtifactID] FROM [EDDSDBO].[Code] (nolock) 
                    WHERE [Name]='" + choiceName + "' AND [CodeTypeId]=" + codeTypeId;

            try { return db.ExecuteSqlStatementAsScalar<int>(sql); }
            catch { return -1; }
        }

        private static int GetCodeTypeOfField(IDBContext db, string fieldName)
        {
            var sql =
                @"SELECT [CodeTypeID] FROM [EDDSDBO].[Field] (nolock) 
                WHERE [DisplayName]='" + fieldName + "'";

            try { return db.ExecuteSqlStatementAsScalar<int>(sql); }
            catch { return -1; }
        }

        #endregion

        #endregion
    }
}
