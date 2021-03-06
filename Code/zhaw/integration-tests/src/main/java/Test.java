import static org.junit.Assert.assertTrue;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import com.sun.jersey.api.client.Client;
import com.sun.jersey.api.client.ClientResponse;
import com.sun.jersey.api.client.WebResource;
import com.sun.jersey.api.client.filter.HTTPBasicAuthFilter;

public class Test {
    private static String userServiceURL = "http://localhost:8888/zhaw-p2abc-webservices/user/";
    private static String verificationServiceURL = "http://localhost:8888/zhaw-p2abc-webservices/verification/protected/";
    private static String verificationServiceURLUnprot = "http://localhost:8888/zhaw-p2abc-webservices/verification/";
    private static String issuanceServiceURL = "http://localhost:8888/zhaw-p2abc-webservices/issuance/protected/";
    private static String issuanceServiceURLUnprot = "http://localhost:8888/zhaw-p2abc-webservices/issuance/";
    private static String credSpecName = "test";
    private static String credSpecURI = "urn%3Afiware%3Aprivacy%3Atest";
    private static String issuanceURI = "urn%3Afiware%3Aprivacy%3Aissuance%3Aidemix";

    /**
     *
     * Performs the whole more or less adopted from the ancient tutorial.
     *
     * Please don't change the order of the calls as it is relevant! To run this
     * test you have to:
     *
     * - have the services running (on tomcat)
     * - have write access to the location where the STORAGE resides
     * - the STORAGE must be EMPTIED/CLEARED before running this.
     * - the configuration needs to be set-up to use FAKE
     * identity and FAKE attribute source (because we check against hardcoded
     * values used in the Fake Providers.
     *
     * (Please don't change the values in Fake Providers without reflecting the
     * changes here and vice versa)).
     *
     * Notes:
     *
     * This integration test tests the whole flow from setup to generation of
     * CredentialSpecification to IssuanceRequest to Verification. However, this
     * test does not check any intermediate results (other than ensuring that
     * the webservices responded with the correct status code) because this test
     * assumes that if the final Verification process succeeds, the test was
     * successful. In other words: This test will obtain a Credential from the
     * Issuance service and verifies the obtained Credential against a
     * PresentationPolicy at the Verification service.
     */
    public static void main(final String[] args) {
        System.out.println("hi there");

        /*
         * Test if all three services are running by calling /status/ * on each
         * service and expecting a 200 response.
         */
        testUserStatus();
        testIssuanceStatus();
        testVerificationStatus();

        /*
         * Ok, if we are here all services are at least running
         */
        /* Test authentication */
        testAuthentication(readTextFile("simpleAuth.xml"));

        /*
         * Get an attributeInfoCollection and convert it to a
         * credentialSpecification
         */
        String attributeInfoCollection = testAttributeInfoCollection();
        String credSpec = testGenCredSpec(attributeInfoCollection);

        /*
         * Store/Get credentialSpecification at issuer
         */
        testStoreCredSpecAtIssuer(credSpec);
        testGetCredSpecFromIssuer();

        /* Store/Get queryRule at issuer */
        testStoreQueryRuleAtIssuer(readTextFile("queryRule.xml"));
        testGetQueryRuleFromIssuer();

        /* Store/Get IssuancePolicy at issuer */
        testStoreIssuancePolicyAtIssuer(readTextFile("issuancePolicy.xml"));
        testGetIssuancePolicyFromIssuer();

        /*
         * * Ok, if we are here the first phase of setup is done.
         */
        /* Generate the SystemParameters */
        String systemParameters = testSetupSystemParametersIssuer();

        /*
         * Store CredentialSpecification at User and Verifier
         */
        testStoreCredSpecAtUser(credSpec);
        testStoreCredSpecAtVerifier(credSpec);

        /*
         * Store SystemParameters at User and Verifier
         */
        testStoreSysParamsAtUser(systemParameters);
        testStoreSysParamsAtVerifier(systemParameters);

        /*
         * Setup IssuerParameters
         */
        String issuerParameters = testSetupIssuerParametersIssuer(readTextFile("issuerParametersInput.xml"));
        System.out.println("--- issuerParameters");
        System.out.println(issuerParameters);

        /*
         * Store IssuerParameters at User and Verifier
         */
        testStoreIssParamsAtUser(issuerParameters);
        testStoreIssParamsAtVerifier(issuerParameters);

        /*
         * * Ok, phase two of setup is done (which means setup is done). * Now
         * the actual issuance protocol can take place.
         */
        String issuanceMessageAndBoolean = testIssuanceRequest(readTextFile("issuanceRequest.xml")); /*
                                                                                                      * Extract
                                                                                                      * issuance
                                                                                                      * message
                                                                                                      */
        String firstIssuanceMessage = testExtractIssuanceMessage(issuanceMessageAndBoolean);
        System.out.println("--- firstIssuanceMessage");
        System.out.println(firstIssuanceMessage);

        /*
         * Issuance steps in the protocol
         */
        String issuanceReturn = testIssuanceStepUser1(firstIssuanceMessage);
        String contextString = getContextString(issuanceReturn);
        System.out.println("--- issuanceReturn");
        System.out.println(issuanceReturn);
        System.out.println(contextString);
        String uiIssuanceReturn = readTextFile("uiIssuanceReturn.xml");
        uiIssuanceReturn = replaceContextString(uiIssuanceReturn, contextString);
        System.out.println("--- uiIssuanceReturn");
        System.out.println(uiIssuanceReturn);
        String secondIssuanceMessage = testIssuanceStepUserUi1(uiIssuanceReturn);
        System.out.println("--- secondIssuanceMessage");
        System.out.println(secondIssuanceMessage);
        String thirdIssuanceMessageAndBoolean = testIssuanceStepIssuer1(secondIssuanceMessage);
        String thirdIssuanceMessage = testExtractIssuanceMessage(thirdIssuanceMessageAndBoolean);
        @SuppressWarnings("unused")
        String fourthIssuanceMessageAndBoolean = testIssuanceStepUser2(thirdIssuanceMessage); /*
                                                                                               * Verification
                                                                                               * stuff
                                                                                               */
        String presentationPolicyAlternatives = testCreatePresentationPolicy(readTextFile("presentationPolicyAlternatives.xml"));
        String presentationReturn = testCreatePresentationToken(presentationPolicyAlternatives);
        contextString = getContextString(presentationReturn);
        System.out.println(contextString);
        String uiPresentationReturn = readTextFile("uiPresentationReturn.xml");
        uiPresentationReturn = replaceContextString(uiPresentationReturn,
                contextString);
        String presentationToken = testCreatePresentationTokenUi(uiPresentationReturn);
        String rPresentationToken = presentationToken.replaceAll(
                "<\\?xml(.*)\\?>", "");
        String rPresentationPolicyAlternatives = presentationPolicyAlternatives
                .replaceAll("<\\?xml(.*)\\?>", "");
        String ppapt = "";
        ppapt += "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
        ppapt += "<PresentationPolicyAlternativesAndPresentationToken xmlns=\"http://abc4trust.eu/wp2/abcschemav1.0\" Version=\"1.0\">";
        ppapt += rPresentationPolicyAlternatives;
        ppapt += rPresentationToken;
        ppapt += "</PresentationPolicyAlternativesAndPresentationToken>";
        String presentationTokenDescription = testVerifyTokenAgainstPolicy(ppapt);
        System.out.println(presentationTokenDescription);
    }

    public static String readTextFile(final String path) {
        try {
            ClassLoader cl = Test.class.getClassLoader();
            File f = new File(cl.getResource(path).getFile());
            BufferedReader br = new BufferedReader(new FileReader(f));
            String lines = "";
            String line = "";
            while ((line = br.readLine()) != null)
                lines += line + "\n";
            br.close();
            System.out.println("*** " + path);
            System.out.println(lines);
            return lines;
        } catch (Exception e) {
            throw new RuntimeException("readTextFile(" + path + ") failed!");
        }
    }

    public static String getContextString(final String input) {
        Pattern pattern = Pattern.compile("<uiContext>(.*)</uiContext>");
        Matcher m = pattern.matcher(input);
        m.find();
        return m.group(1);
    }

    public static String replaceContextString(final String input,
            final String contextString) {
        return input.replaceAll("REPLACE-THIS-CONTEXT", contextString);
    }

    public static Client getClient() {
        Client c = Client.create();
        c.addFilter(new HTTPBasicAuthFilter("api", "jura"));
        return c;
    }

    public static String testVerifyTokenAgainstPolicy(final String ppapt) {
        Client client = getClient();
        WebResource webResource = client.resource(verificationServiceURLUnprot
                + "verifyTokenAgainstPolicy");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, ppapt);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testCreatePresentationTokenUi(final String pr) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "createPresentationTokenUi");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, pr);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testCreatePresentationToken(final String ppa) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "createPresentationToken");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, ppa);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testCreatePresentationPolicy(final String ppa) {
        Client client = getClient();
        WebResource webResource = client.resource(verificationServiceURLUnprot
                + "createPresentationPolicy");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, ppa);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testIssuanceStepUser2(final String im) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "issuanceProtocolStep");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, im);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testIssuanceStepIssuer1(final String im) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURLUnprot
                + "issuanceProtocolStep");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, im);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testIssuanceStepUserUi1(final String uir) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "issuanceProtocolStepUi");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, uir);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testIssuanceStepUser1(final String im) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "issuanceProtocolStep");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, im);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testExtractIssuanceMessage(final String imab) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "extractIssuanceMessage");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, imab);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testIssuanceRequest(final String ir) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURLUnprot
                + "issuanceRequest");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, ir);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static void testStoreIssParamsAtUser(final String p) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "issuerParameters/store/" + issuanceURI);
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, p);
        assertOk(response);
    }

    public static void testStoreIssParamsAtVerifier(final String p) {
        Client client = getClient();
        WebResource webResource = client.resource(verificationServiceURL
                + "storeIssuerParameters/" + issuanceURI);
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, p);
        assertOk(response);
    }

    public static String testSetupIssuerParametersIssuer(final String input) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "setupIssuerParameters/");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, input);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static void testStoreSysParamsAtUser(final String sp) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "systemParameters/store");
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, sp);
        assertOk(response);
    }

    public static void testStoreSysParamsAtVerifier(final String sp) {
        Client client = getClient();
        WebResource webResource = client.resource(verificationServiceURL
                + "storeSystemParameters/");
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, sp);
        assertOk(response);
    }

    public static void testStoreCredSpecAtUser(final String credSpec) {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL
                + "credentialSpecification/store/" + credSpecURI);
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, credSpec);
        assertOk(response);
    }

    public static void testStoreCredSpecAtVerifier(final String credSpec) {
        Client client = getClient();
        WebResource webResource = client.resource(verificationServiceURL
                + "storeCredentialSpecification/" + credSpecURI);
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, credSpec);
        assertOk(response);
    }

    public static String testSetupSystemParametersIssuer() {
        String uri = "setupSystemParameters/?securityLevel=80&cryptoMechanism=urn:abc4trust:1.0:algorithm:idemix";
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL + uri);
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testGetIssuancePolicyFromIssuer() {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "issuancePolicy/get/" + credSpecURI);
        ClientResponse response = webResource.get(ClientResponse.class);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static void testStoreIssuancePolicyAtIssuer(final String ip) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "issuancePolicy/store/" + credSpecURI);
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, ip);
        assertOk(response);
    }

    public static String testGetQueryRuleFromIssuer() {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "queryRule/get/" + credSpecURI);
        ClientResponse response = webResource.get(ClientResponse.class);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static void testStoreQueryRuleAtIssuer(final String queryRule) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "queryRule/store/" + credSpecURI);
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, queryRule);
        assertOk(response);
    }

    public static String testGetCredSpecFromIssuer() {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "credentialSpecification/get/" + credSpecURI);
        ClientResponse response = webResource.get(ClientResponse.class);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static void testStoreCredSpecAtIssuer(final String credSpec) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "credentialSpecification/store/" + credSpecURI);
        ClientResponse response = webResource.type("application/xml").put(
                ClientResponse.class, credSpec);
        assertOk(response);
    }

    public static String testAuthentication(final String authRequest) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURLUnprot
                + "testAuthentication");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, authRequest);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testGenCredSpec(final String attributeInfoCollection) {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "credentialSpecification/generate");
        ClientResponse response = webResource.type("application/xml").post(
                ClientResponse.class, attributeInfoCollection);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static String testAttributeInfoCollection() {
        Client client = getClient();
        WebResource webResource = client.resource(issuanceServiceURL
                + "attributeInfoCollection/" + credSpecName);
        ClientResponse response = webResource.get(ClientResponse.class);
        assertOk(response);
        return response.getEntity(String.class);
    }

    public static void testUserStatus() {
        Client client = getClient();
        WebResource webResource = client.resource(userServiceURL + "status");
        ClientResponse response = webResource.get(ClientResponse.class);
        assertOk(response);
    }

    public static void testIssuanceStatus() {
        Client client = getClient();
        WebResource webResource = client
                .resource(issuanceServiceURL + "status");
        ClientResponse response = webResource.get(ClientResponse.class);
        assertOk(response);
    }

    public static void testVerificationStatus() {
        Client client = getClient();
        WebResource webResource = client.resource(verificationServiceURL
                + "status");
        ClientResponse response = webResource.get(ClientResponse.class);
        assertOk(response);
    }

    public static void assertOk(final ClientResponse response) {
        if (response.getClientResponseStatus() != ClientResponse.Status.OK) {
            System.out.println("-- NOT OK --");
            System.out.println(response.getStatus());
            System.out.println(response.getEntity(String.class));
        }
        assertTrue(response.getClientResponseStatus()
                == ClientResponse.Status.OK);
    }
}
