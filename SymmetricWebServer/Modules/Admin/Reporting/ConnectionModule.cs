using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;
using WebServer.Database;
using Nancy.ModelBinding;
using Mono.Data.Sqlite;

namespace WebServer.Modules.Admin.Reporting
{
    public class ConnectionModule : EditControlModule
    {
        protected const string PostRefresh = "refresh";
        protected const string PostTest = "test";

        public ConnectionModule()
            : base(AccessLevels.Admin, "Connections", "connection", "connections", "admin/reporting/configureconnection")
        {

        }

        protected override List<BasicEntry> Items()
        {
            return new DBContent().BasicConnections;
        }

        protected override object ProcessAddItem()
        {
            return new ConnectionItem();
        }

        protected override bool ProcessDeleteItem(int id, out string errormessage)
        {
            return new DBContent().DeleteConnection(id, out errormessage);
        }

        protected override object ProcessEditItem(int id)
        {
            ConnectionItem item = new DBContent().GetConnection(id);
            if (item != null)
            {
                item.RefreshDatabases();
            }
            return item;
        }

        protected override ApplyResult ProcessApplyItem(ref object obj, out string errorMessage, out string successMessage, out bool edited)
        {
            errorMessage = "";
            successMessage = "";
            edited = false;
            string action = this.Request.Form.action.Value.ToLower();
            int objectID = -1;
            if (this.Request.Form.objectID != null)
            {
                objectID = int.Parse(this.Request.Form.objectID.Value.ToString());
            }

            string defaultDatabase = "";
            if (this.Request.Form.DefaultDatabase != null)
            {
                defaultDatabase = this.Request.Form.DefaultDatabase.Value;
            }

            int port = 0;
            bool portError = false;
            if (this.Request.Form.ConnPort != null)
            {
                if (!int.TryParse(this.Request.Form.ConnPort.Value, out port))
                {
                    portError = true;
                }
            }
            ConnectionItem.ConnectionTypes type =
                    this.Request.Form.ConnectionTypes.Value == "mysql" ? ConnectionItem.ConnectionTypes.MySQL : ConnectionItem.ConnectionTypes.MSSQL;

            ConnectionItem item = new ConnectionItem(objectID,
                                                    this.Request.Form.Name.Value,
                                                    this.Request.Form.Host.Value,
                                                    port,
                                                    this.Request.Form.Username.Value,
                                                    this.Request.Form.Password.Value,
                                                    type,
                                                    defaultDatabase);
            obj = item;

            bool showRefreshMessage = false;
            switch (action)
            {
                case BaseWebModule.PostSave:
                    bool result = false;
                    if (!portError)
                    {
                        result = new DBContent().SaveConnection(item, out errorMessage);
                        edited = item.ID > 0;
                    }
                    else
                    {
                        errorMessage = "Invalid Port.";
                    }

                    if (result)
                    {
                        return ApplyResult.Save;
                    }
                    break;
                case ConnectionModule.PostRefresh:
                    showRefreshMessage = true;
                    break;
                case BaseWebModule.PostCancel:
                    return ApplyResult.Cancel;
                case ConnectionModule.PostTest:
                    if (ConnectionItem.TestConnection(item, out errorMessage))
                    {
                        successMessage = "Successfully connected to the database.";
                    }
                    break;
            }

            string message;
            if(item.RefreshDatabases(out message))
            {
                if (showRefreshMessage)
                {
                    successMessage = "Successfully refreshed.";
                }
            }
            else
            {
                if (showRefreshMessage)
                {
                    errorMessage = message;
                }
            }
            return ApplyResult.Message;
        }
    }
}
