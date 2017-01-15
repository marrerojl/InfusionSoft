using System;
using System.Collections.Generic;
using Infusion.Interface;
using Infusion.Interface.Support;
using Infusion.Interface.BusinessObjects;
using Infusion.Interface.BusinessObjects.Data.FindByField;
using Infusion.Interface.BusinessObjects.Contact.RetriveAContact;

namespace TestInfusionSoft
{
    class Program
    {
        static void Main(string[] args)
        {
            string loginName = string.Empty;
            string userPassword = string.Empty;
            InfSoftConnection connection = new InfSoftConnection();
            connection.ISoftAuthURL = @"https://signin.infusionsoft.com/app/oauth/authorize";
            connection.ISoftRESTURL = @"https://api.infusionsoft.com/crm/rest/v1";
            connection.ISoftTokenURL = @"https://api.infusionsoft.com/token";
            connection.ISoftXmlRpcURL = @"https://api.infusionsoft.com/crm/xmlrpc/v1";

            Console.Write("Provide application key: ");
            connection.ISoftKey =  Console.ReadLine();
            Console.Write("Provide application secret: ");
            connection.ISoftSecret = Console.ReadLine();
            Console.Write("Register Callback URL: ");
            connection.ISoftRedirectUrl = Console.ReadLine();
            Console.Write("Provide InfusionSoft account login name: ");
            loginName = Console.ReadLine();
            Console.Write("Provide InfusionSoft account login name password: ");
            userPassword = Console.ReadLine();

            if (!string.IsNullOrEmpty(connection.ISoftKey) && !string.IsNullOrEmpty(connection.ISoftSecret) && !string.IsNullOrEmpty(loginName) && !string.IsNullOrEmpty(userPassword))
            {
                Access infuAccess = new Access(connection, true);
                AccessTokenContainer accessToken = infuAccess.AuthorizeAndGetToken(loginName, userPassword);
                if (accessToken.Status)
                {
                    //authentication and token obtained
                    Console.WriteLine(string.Format(@"Authentication and token acquired succesfully. Access Token: {0}  Expiration in seconds: {1}  Refresh Token: {2}", accessToken.AccessTokenInfo.Access_Token, accessToken.AccessTokenInfo.Expires_In, accessToken.AccessTokenInfo.Refresh_Token));

                    Console.Write("To test Contact service 'Retrive A Contact' provide the ID: ");
                    string contactID = Console.ReadLine();
                    RetriveAContact(infuAccess, accessToken.AccessTokenInfo, contactID);
                    Console.WriteLine();

                    Console.WriteLine("To test Data service 'Find By Field' provide last name to search by and two field names from the 'Contact' table to retrive.");
                    Console.Write("Contact last name to query: ");
                    string contactFieldToQuery = Console.ReadLine();
                    Console.Write("Contact field name to retrive: ");
                    string contactField1 = Console.ReadLine();
                    Console.Write("Contact field name to retrive: ");
                    string contactField2 = Console.ReadLine();

                    FindByField(infuAccess, accessToken.AccessTokenInfo, contactFieldToQuery, contactField1, contactField2);                
                }
                else
                {
                    Console.WriteLine(accessToken.Message);
                }

            }
            else
            {
                Console.WriteLine("Missing parameters values.");
            }
            Console.ReadKey();
        }

        static void FindByField(Access infuAccess, AccessToken accessToken, string contactFieldToQuery, string contactField1, string contactField2)
        {
            FindByFieldRequest.MethodCall methodCall = new FindByFieldRequest.MethodCall();
            methodCall.Params = new FindByFieldRequest.Params { Param = new List<FindByFieldRequest.Param>() };
            FindByFieldRequest.Param privateKey = new FindByFieldRequest.Param();
            privateKey.Value = new FindByFieldRequest.Value { String = GlobalObjects.iSoftKey };
            methodCall.Params.Param.Add(privateKey);
            FindByFieldRequest.Param tableName = new FindByFieldRequest.Param();
            tableName.Value = new FindByFieldRequest.Value { String = "Contact" };
            methodCall.Params.Param.Add(tableName);
            FindByFieldRequest.Param limit = new FindByFieldRequest.Param();
            limit.Value = new FindByFieldRequest.Value { Int = "10" };           //1000 is the max per page, you can use "page" to move to the next 1000
            methodCall.Params.Param.Add(limit);
            FindByFieldRequest.Param page = new FindByFieldRequest.Param();
            page.Value = new FindByFieldRequest.Value { Int = "0" };                //this is the first page, using this and "limit" you can move throug pages to get all records
            methodCall.Params.Param.Add(page);
            FindByFieldRequest.Param fieldName = new FindByFieldRequest.Param();
            fieldName.Value = new FindByFieldRequest.Value { String = "LastName" };
            methodCall.Params.Param.Add(fieldName);
            FindByFieldRequest.Param fieldValue = new FindByFieldRequest.Param();
            fieldValue.Value = new FindByFieldRequest.Value { String = contactFieldToQuery };
            methodCall.Params.Param.Add(fieldValue);
            FindByFieldRequest.Param returnFields = new FindByFieldRequest.Param();
            returnFields.Value = new FindByFieldRequest.Value { Array = new FindByFieldRequest.Array { Data = new FindByFieldRequest.Data { Value = new List<FindByFieldRequest.Value>() } } };
            FindByFieldRequest.Value firstName = new FindByFieldRequest.Value { String = contactField1 };
            FindByFieldRequest.Value company = new FindByFieldRequest.Value { String = contactField2 };
            returnFields.Value.Array.Data.Value.Add(firstName);
            returnFields.Value.Array.Data.Value.Add(company);
            methodCall.Params.Param.Add(returnFields);

            FindByFieldResponse contact = infuAccess.FindByField(accessToken, methodCall);                       //Get contact information via "Data" service
            if (contact.Status)
            {
                foreach (FindByFieldResponse.Value value in contact.ResponseData.Value)
                {
                    foreach (FindByFieldResponse.Member member in value.Struct.Member)
                    {
                        Console.WriteLine(string.Format("Field name query: {0}   Field value: {1}", member.Name, member.Value));
                    }
                }
            }
            else
            {
                Console.WriteLine(contact.Message);
            }
        }

        static void RetriveAContact(Access infuAccess, AccessToken accessToken, string contactID)
        {
            RetriveAContactRequest.MethodCall methodCall = new RetriveAContactRequest.MethodCall();
            methodCall.Params = new RetriveAContactRequest.Params { Param = new List<RetriveAContactRequest.Param>() };
            RetriveAContactRequest.Param privateKey = new RetriveAContactRequest.Param();
            privateKey.Value = new RetriveAContactRequest.Value { String = GlobalObjects.iSoftKey };
            methodCall.Params.Param.Add(privateKey);
            RetriveAContactRequest.Param contactIDNumber = new RetriveAContactRequest.Param();
            contactIDNumber.Value = new RetriveAContactRequest.Value { Int = contactID };
            methodCall.Params.Param.Add(contactIDNumber);
            RetriveAContactRequest.Param returnFields = new RetriveAContactRequest.Param();
            returnFields.Value = new RetriveAContactRequest.Value { Array = new RetriveAContactRequest.Array { Data = new RetriveAContactRequest.Data { Value = new List<RetriveAContactRequest.Value>() } } };
            RetriveAContactRequest.Value firstName = new RetriveAContactRequest.Value { String = "FirstName" };
            RetriveAContactRequest.Value company = new RetriveAContactRequest.Value { String = "Company" };
            returnFields.Value.Array.Data.Value.Add(firstName);
            returnFields.Value.Array.Data.Value.Add(company);
            methodCall.Params.Param.Add(returnFields);

            RetriveAContactResponse contact = infuAccess.RetriveAContact(accessToken, methodCall);
            if (contact.Status)
            {
                foreach (RetriveAContactResponse.Member member in contact.ResponseValue.Struct.Member)
                {
                    Console.WriteLine(string.Format("Field name query: {0}   Field value: {1}", member.Name, member.Value));
                }           
            }
            else
            {
                Console.WriteLine(contact.Message);
            }
        }
    }
}
