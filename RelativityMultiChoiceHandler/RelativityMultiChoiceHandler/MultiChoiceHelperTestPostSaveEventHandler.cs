using kCura.EventHandler;
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
    public class MultiChoiceHelperTestPostSaveEventHandler : PostSaveEventHandler
    {
        private const string FIELD_NAME_1 = "Multi Choice Test 1";
        private const string FIELD_NAME_2 = "Multi Choice Test 2";

        public override FieldCollection RequiredFields
        {
            get
            {
                FieldCollection f = new FieldCollection();
                f.Add(new kCura.EventHandler.Field(FIELD_NAME_1));
                f.Add(new kCura.EventHandler.Field(FIELD_NAME_2));
                return f;
            }
        }

        public override Response Execute()
        {
            Response r = new Response();
            try
            {
                //definitions
                int workspaceId = this.Helper.GetActiveCaseID();
                IDBContext dbc = this.Helper.GetDBContext(workspaceId);
                IRSAPIClient proxy = this.Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
                proxy.APIOptions.WorkspaceID = workspaceId;
                proxy.APIOptions.StrictMode = true;


                //Content for field 1
                List<string> fieldChoices1 = new List<string>()
                {
                    "A",
                    "B",
                    "C",
                    "D"
                };

                //Content for field 2
                List<string> fieldChoices2 = new List<string>()
                {
                    "A/Aa/Aardvark",
                    "A/Ap/Apple",
                    "A/An/Anexsys",
                    "B/Ba/Bat"
                };

                RDO obj = proxy.Repositories.RDO.ReadSingle(this.ActiveArtifact.ArtifactID);

                obj.ArtifactTypeID = this.ActiveArtifact.ArtifactTypeID;

                obj.Fields.Add(new kCura.Relativity.Client.DTOs.FieldValue(FIELD_NAME_1, MultiChoiceHelpers.CreateAndGetMultiChoices(dbc, proxy, FIELD_NAME_1, fieldChoices1)));
                obj.Fields.Add(new kCura.Relativity.Client.DTOs.FieldValue(FIELD_NAME_2, MultiChoiceHelpers.CreateAndGetMultiChoices(dbc, proxy, FIELD_NAME_2, fieldChoices2)));

                WriteResultSet<RDO> results = proxy.Repositories.RDO.Update(obj);
                if (!results.Success)
                {
                    r.Success = false;
                    r.Message = "Unable to update fields: " + results.Message;
                    return r;
                }

                r.Success = true;
            }
            catch (Exception e)
            {
                r.Success = false;
                r.Message = e.Message;
            }
            return r;
        }
    }
}
