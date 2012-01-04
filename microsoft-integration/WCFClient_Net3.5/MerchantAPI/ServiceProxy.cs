﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

using Earthport.MerchantAPI.Extensions;

namespace Earthport.MerchantAPI
{
    /// <summary>
    /// Used to invoke operations on the Earthport Merchant Web Service.
    /// </summary>
    /// <remarks>
    /// The ServiceProxy is the primary class used to invoke operations on the Earthport Merchant Web Service.  A ServiceProxy must first be 
    /// instantiated by providing a service URL, username and password.  Methods can then be called which automatically invoke the appropriate web 
    /// service operation and return a result. The general format of a method is:
    /// 
    /// [OperationResponseType] Operation([OperationRequestType] operationRequest)
    /// 
    /// Prior to calling the corresponding web service operation, the operationRequest is validated against its schema.  If the request is invalid, 
    /// an XmlSchemaValidationException is thrown.  If the request is valid, but the web service operation returns with an error, a 
    /// ServiceErrorException is thrown.
    /// </remarks>
    public class ServiceProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxy">ServiceProxy</see> class
        /// </summary>
        /// <param name="serviceUrl">The URL of the web service (as supplied by Earthport)</param>
        /// <param name="username">The service username (as supplied by Earthport)</param>
        /// <param name="password">The service password (as supplied by Earthport)</param>
        public ServiceProxy(string serviceUrl, string username, string password)
        {
            // create an instance of the autogenerated service proxy and set the url
            _client = new MerchantAPIClient();
            _client.Endpoint.Address = new EndpointAddress(serviceUrl);

            // set the credentials which are used to generate the WS-Security Username Token
            _client.ClientCredentials.UserName.UserName = username;
            _client.ClientCredentials.UserName.Password = password;

            // By default, a security timestamp is expected in the response and if it not present, a MessageSecurityException is thrown.
            // Since the web service does not include a timestamp in the response, to prevent an exception being thrown the following code modifies
            // the binding such that a timestamp is not expected
            BindingElementCollection elements = _client.Endpoint.Binding.CreateBindingElements();
            elements.Find<SecurityBindingElement>().IncludeTimestamp = false;
            _client.Endpoint.Binding = new CustomBinding(elements);

            // load up schemas embedded in the assembly
            _schemaSet = new XmlSchemaSet();
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.components.bankBaseTypes-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.components.coreTypes-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.components.financialTransactionBaseTypes-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.components.identityBaseTypes-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.components.payoutBaseTypes-1.2.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.addBeneficiaryBankAccount-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.createOrUpdateUser-1.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.createUser-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.deleteBeneficiaryBankAccount-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.disableUser-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.echo-1.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.errorResponse-1.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getBalance-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getBeneficiaryBankAccount-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getBeneficiaryBankAccountRequiredData-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getFXQuote-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getIndicativeFXQuote-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getPayerIdentity-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getStatement-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.getTransactionDetail-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.listBeneficiaryBankAccounts-4.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.payoutRequest-4.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.searchTransactions-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.updatePayerIdentity-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.validateBeneficiaryBankAccount-3.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.validateCredit-2.0.xsd"));
            _schemaSet.Add(null, GetEmbeddedSchema("Earthport.MerchantAPI.ServiceMetadata.services.validatePayerIdentity-1.0.xsd"));
            // This code is here for the client to work in a testing environment should NOT be be deployed in a production environment.
            // Because the test application server uses a self-signed certificate whose common name does not match the host name, it is not
            // trusted and a SecurityNegotiationException is thrown.  The following line specifies a delegate to override the certificate validation 
            // This code and the 
            #if DEBUG
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OverrideCertificateValidation);  
            #endif
        }

        /// <summary>
        /// Used to create a virtual account in the system.  Takes the merchant's reference (e.g. johnxcitizen1), currency that the account should
        /// use, and a sub-brand as input and returns a UserID token.
        /// </summary>
        /// <param name="createUser">A CreateUserType request</param>
        /// <returns>A response of type CreateUserTypeResponse</returns>
        public CreateUserResponseType CreateUser(CreateUserType createUser)
        {
            return InvokeService<CreateUserResponseType>(createUser);
        }
        /// <summary>
        /// Used to create a virtual account in the system.  Takes the merchant's reference (e.g. johnxcitizen1), currency that the account should
        /// use, and a sub-brand as input and returns a UserID token.
        /// </summary>
        /// <param name="createUser">A CreateUserType request</param>
        /// <returns>A response of type CreateUserTypeResponse</returns>
        public CreateOrUpdateUserResponseType CreateOrUpdateUser(CreateOrUpdateUserType createUser)
        {
            return InvokeService<CreateOrUpdateUserResponseType>(createUser);
        }

        /// <summary>
        /// Used to get a list of fields that are required to add a beneficiary account, given a service level, country and currency.
        /// </summary>
        /// <param name="getBeneficiaryBankAccountRequiredData">A GetbeneficiaryAccountRequiredDataType request</param>
        /// <returns>A response of type GetBeneficiaryAccountRequiredDataResponseType</returns>
        public GetBeneficiaryBankAccountRequiredDataResponseType GetBeneficiaryBankAccountRequiredData(GetBeneficiaryBankAccountRequiredDataType getBeneficiaryBankAccountRequiredData)
        {
            return InvokeService<GetBeneficiaryBankAccountRequiredDataResponseType>(getBeneficiaryBankAccountRequiredData);
        }

        /// <summary>
        /// Used to add a beneficiary bank account and associate it with a specified UserID token.  Takes the UserID token and the bank details, along with an optional 
        /// merchant bank ID of the beneficiary bank account.  
        /// Returns a unique UsersBankID that identified the new beneficiary bank account.
        /// </summary>
        /// <param name="addBeneficiaryBankAccount">An AddBeneficiaryBankAccountType request</param>
        /// <returns>A response of type AddBeneficiaryBankAccountResponseType</returns>
        public AddBeneficiaryBankAccountResponseType AddBeneficiaryBankAccount(AddBeneficiaryBankAccountType addBeneficiaryBankAccount)
        {
            return InvokeService<AddBeneficiaryBankAccountResponseType>(addBeneficiaryBankAccount);
        }

        /// <summary>
        /// Used to validate beneficiary bank account details without adding them into the system.  Operates as per AddBank but is "read-only".
        /// </summary>
        /// <param name="validateBeneficiary">A ValidateBeneficiaryBankAccountType request</param>
        /// <returns>
        /// A response of type ValidateBeneficiaryBankAccountResponseType (there are no properties on this class.  If a response is 
        /// returned, that means the bank account was validated successfully.  If the validation failed, a ServiceErrorException is thrown
        /// with details of why the validation failed.
        /// </returns>
        public ValidateBeneficiaryBankAccountResponseType ValidateBeneficiaryBankAccount(ValidateBeneficiaryBankAccountType validateBeneficiaryBankAccount)
        {
            return InvokeService<ValidateBeneficiaryBankAccountResponseType>(validateBeneficiaryBankAccount);
        }

        /// <summary>
        /// Gets a guaranteed FX rate between two currencies.  Takes a sell Monetary Amount (currency and amount) and a buy currency.  
        /// Will return an FXTicketID and the exchange rate.  Valid for 30 seconds.
        /// </summary>
        /// <param name="getFXQuote">A GetFXQuote request</param>
        /// <returns>A response of type GetFXQuoteResponse</returns>
        public GetFXQuoteResponseType GetFXQuote(GetFXQuoteType getFXQuote)
        {
            return InvokeService<GetFXQuoteResponseType>(getFXQuote);
        }

        /// <summary>
        /// Pays money to an already registered beneficiary bank account.  takes a UsersBankID token to identify a benficiary bank account, a merchant reference, and an optional id for
        /// the bank account to payout to, an optional FXTicketID, and a Monetary Amount (currency, amount) to payout
        /// </summary>
        /// <param name="payoutRequest">A PayoutRequest request</param>
        /// <returns>A response of type PayoutRequestResponse</returns>
        public PayoutRequestResponseType PayoutRequest(PayoutRequestType payoutRequest)
        {
            return InvokeService<PayoutRequestResponseType>(payoutRequest);
        }

        /// <summary>
        /// List a set of active beneficiary bank accounts for a UserID token provided. Takes a UserID token which
        /// can consist of an EP User ID (VAN) and/or merchant user ID.
        /// </summary>
        /// <param name="listBeneficiaryAccountsRequest">A ListBeneficiaryBankAccounts request</param>
        /// <returns>A response of type ListBeneficiaryBankAccountResponse</returns>
        public ListBeneficiaryBankAccountsResponseType ListBeneficiaryAccounts(ListBeneficiaryBankAccountsType listBeneficiaryAccountsRequest)
        {
            return InvokeService<ListBeneficiaryBankAccountsResponseType>(listBeneficiaryAccountsRequest);
        }

        /// <summary>
        /// Delete a beneficiary bank account for a UsersBankID token provided. Takes a UsersBankID which will contain
        /// the BenBankID token which is the bank account that will be deleted.
        /// </summary>
        /// <param name="deleteBeneficiaryBankAccountRequest">A DeleteBeneficiaryBankAccount request</param>
        /// <returns>A response of type DeleteBeneficiaryBankAccountResponse</returns>
        public DeleteBeneficiaryBankAccountResponseType DeleteBeneficiaryBankAccount(DeleteBeneficiaryBankAccountType deleteBeneficiaryBankAccountRequest)
        {
            return InvokeService<DeleteBeneficiaryBankAccountResponseType>(deleteBeneficiaryBankAccountRequest);
        }

        /// <summary>
        /// Used to test the EPS 2 web services. Takes in any string as a request and outputs the same string
        /// as a response.
        /// </summary>
        /// <param name="echoRequest">An EchoRequest request</param>
        /// <returns>A response of type EchoResponse</returns>
        public EchoResponseType Echo(EchoType echoRequest)
        {
            return InvokeService<EchoResponseType>(echoRequest);
        }

        /// <summary>
        /// This function is used by customers to list all of the details for the specified beneficiary bank account.
        /// 
        /// The message needs to contain a mandatory user bank ID which contains the 'userID' token and a 'benBankID' 
        ///	token representing the beneficiary	bank account to get.
        /// </summary>
        /// <param name="getBeneficiaryBankAccountRequest">A getBeneficiaryBankAccount request</param>
        /// <returns>A response of type getBeneficiaryBankAccountResponse</returns>
        public getBeneficiaryBankAccountResponseType GetBeneficiaryBankAccount(getBeneficiaryBankAccountType getBeneficiaryBankAccountRequest)
        {
            return InvokeService<getBeneficiaryBankAccountResponseType>(getBeneficiaryBankAccountRequest);
        }

        /// <summary>
        /// This function is used by customers to mark a user as closed/deleted.  
		/// This will prevent subsequent payouts/deposits to/from this user from succeeding.  
		///	Note that it is not possible to re-activate a closed account at a later date.
        /// </summary>
        /// <param name="disableUserRequest">A DisableUser request</param>
        /// <returns>A response of type DisableUserResponseType</returns>
        public DisableUserResponseType DisableUser(DisableUserType disableUserRequest)
        {
            return InvokeService<DisableUserResponseType>(disableUserRequest);
        }

        /// <summary>
        /// This function is used by Earthport merchants
		///	to retrieve a set of balance figures represented by a
		///	monetary amount for a given user account for each
		///	currency registered with the user account.
        ///
		///	If a single currency is registered for a user account,
		///	only one monetary value will be returned. If multiple
		///	currencies are registered, then all balances for each
		///	currency will be returned.
        /// </summary>
        /// <param name="getBalanceRequest">A GetBalance request</param>
        /// <returns>A response of type GetBalanceResponseType</returns>
        public GetBalanceResponseType GetBalance(GetBalanceType getBalanceRequest)
        {
            return InvokeService<GetBalanceResponseType>(getBalanceRequest);
        }

        /// <summary>
        /// This function is used by customers 
		///	to verify that the credit details sent during a deposit/refund 
		///	notification are correct and valid.
        /// </summary>
        /// <param name="validateCreditRequest">A ValidateCredit request</param>
        /// <returns>A response of type ValidateCreditResponseType</returns>
        public ValidateCreditResponseType ValidateCredit(ValidateCreditType validateCreditRequest)
        {
            return InvokeService<ValidateCreditResponseType>(validateCreditRequest);
        }

        /// <summary>
        /// This function is used by customers 
        ///	to verify view details of a transaction
        /// </summary>
        /// <param name="getTransactionDetailRequest">A GetTransactionDetailType request</param>
        /// <returns>A response of type GetTransactionDetailResponseType</returns>
        public GetTransactionDetailResponseType GetTransactionDetail(GetTransactionDetailType getTransactionDetailRequest)
        {
            return InvokeService<GetTransactionDetailResponseType>(getTransactionDetailRequest);
        }
        

        /// <summary>
        /// Function used by customers to retrieve a statement of transactions
        /// for items appearing on their van
        /// </summary>
        /// <param name="getStatementRequest">A GetStatementType request</param>
        /// <returns>A response of type GetStatementResponseType</returns>
        public GetStatementResponseType GetStatement(GetStatementType getStatementRequest)
        {
            return InvokeService<GetStatementResponseType>(getStatementRequest);
        }

        /// <summary>
        /// Function used to search for transactions on a merchant VAN
        /// </summary>
        /// <param name="searchTransactionsRequest">A SearchTransactionsType request</param>
        /// <returns>A response of type SearchTransactionsResponseType</returns>
        public SearchTransactionsResponseType SearchTransactions(SearchTransactionsType searchTransactionsRequest)
        {
            return InvokeService<SearchTransactionsResponseType>(searchTransactionsRequest);
        }        
        
        /// <summary>
        /// Generic method to invoke a web service operation. Serializes the request object to XML, validates the XML against the corresponding
        /// schema, invokes the service and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type of the expected response</typeparam>
        /// <param name="requestObj">The request object</param>
        /// <returns>A deserialized object of the specified response type T</returns>
        /// <exception cref="XmlSchemaValidationException">Thrown if the validation of the serialized XML againsts the correspoding schema fails.</exception>
        /// <exception cref="ServiceErrorException">Thrown if the service responds with an error (ack value of 'failure').</exception>
        private T InvokeService<T>(object requestObj)
        {
            // serialize the request object to equivalent xml
            XmlElement serializedXml = requestObj.SerializeAndValidate(_schemaSet);

            // call the submitDocument web service operation
            Response response = _client.submitDocument(serializedXml);

            if (response.ack == AckType.failure)
            {
                // throw an exception if the response has an AckType of failure
                throw new ServiceErrorException(
                    "The service returned an error.  The 'ServiceErrorResponse' property contains details about this error. ErrorMsg=" + response.Any.InnerXml.ToString(),
                    response.Any.DeserializeToObject<ErrorResponseType>()
                    );
            }

            // deserialize the response xml to the appropriate response type and return
            return response.Any.DeserializeToObject<T>();
        }

        /// <summary>
        /// Gets an XML schema embedded in the assembly
        /// </summary>
        /// <param name="name">The name of the schema</param>
        /// <returns>An XmlReader representing the schema</returns>
        private XmlReader GetEmbeddedSchema(string name)
        {
            // get the specified embedded assembly and return an XmlReader for it
            return new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name));
        }

        /// <summary>
        /// This code is here for the client to work in a testing environment should NOT be be deployed in a production environment.
        /// This method overrides the certificate validation which ordinarily fails in a test envvironment where the host has an X509 certificate
        /// that is not trusted.
        /// </summary>
        #if DEBUG
        private bool OverrideCertificateValidation(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
        #endif

        private MerchantAPIClient _client; // an instance of the generated service proxy
        private XmlSchemaSet _schemaSet; // set of schemas to validate serialized request XML against
    }
}