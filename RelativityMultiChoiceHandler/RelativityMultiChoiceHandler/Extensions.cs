using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelativityMultiChoiceHandler
{
    public static class Extensions
    {
        /// <summary>
        /// Add choices to a MultiChoiceFieldValueList based on a list of choice objects
        /// </summary>
        /// <param name="choiceList">this</param>
        /// <param name="choices">A list of choice objects to add</param>
        /// <param name="proxy">IRSAPIClient with workspace ID pre-set</param>
        public static void AddChoices(this MultiChoiceFieldValueList choiceList, List<kCura.Relativity.Client.DTOs.Choice> choices, IRSAPIClient proxy)
        {
            if (choices.Count <= 0) return;
            try
            {
                var resultSet = proxy.Repositories.Choice.Create(choices);

                if (resultSet.Success)
                {
                    foreach (var choice in resultSet.Results)
                    {
                        if (choice.Success) { choiceList.Add(choice.Artifact); }
                    }
                }
                else
                {
                    foreach (var r in resultSet.Results)
                    {
                        if (!r.Success)
                        {
                            throw new Exception("Creation of choices failed. Error: " + r.Message);
                        }
                    }

                    throw new Exception("Creation of choices failed. " + resultSet.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create choices: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Add choices to a MultiChoiceFieldValueList based on a list of choice IDs
        /// </summary>
        /// <param name="choiceList">this</param>
        /// <param name="choiceIds">A list of choice IDs to add</param>
        /// <param name="proxy">IRSAPIClient with workspace ID pre-set</param>
        public static void AddChoiceIds(this MultiChoiceFieldValueList choiceList, List<int> choiceIds, IRSAPIClient proxy)
        {
            choiceList.UpdateBehavior = MultiChoiceUpdateBehavior.Replace;

            foreach (var c in choiceIds)
            {
                try { choiceList.Add(proxy.Repositories.Choice.ReadSingle(c)); }
                catch { continue; }
            }
        }

    }
}
