﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ABC4TrustSmartCard
{
  
  interface IABC4TrustSmartCard
  {
    /// <summary>
    /// Returns a 64-byte ASCII string containing technical information on the resident smart card application.
    /// Works in any mode.
    /// </summary>
    /// <param name="version">The version as a string as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetVersion(out String version);

    /// <summary>
    /// Returns mode. Works in any mode.
    /// </summary>
    /// <param name="mode">The CardMode enum as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetMode(out CardMode mode);

    /// <summary>
    /// If pin == PIN on card, returns deviceID. Only works in root and working modes.
    /// </summary>
    /// <param name="pin">The card pin code</param>
    /// <param name="deviceID">The device id as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetDeviceID(byte[] pin, out byte[] deviceID);

    /// <summary>
    /// A card in virgin mode is protected by an 8-byte access code generated by
    /// CryptoExperts and made available at delivery time.
    /// </summary>
    /// <param name="accCode">The root access code as specified</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetRootMode(byte[] accCode);
    
    /// <summary>
    /// After personalization is performed by the root authority, the
    /// first execution of SET WORKING MODE irreversibly puts the card in working mode.
    /// 
    /// Only works if the card is in root mode.
    /// </summary>
    /// <returns>ErrorCode</returns>
    ErrorCode SetWorkingMode();

    /// <summary>
    /// Irrespective of its current mode, the card can be put back in virgin
    /// mode at any time. A hard-coded secret key is used to authenticate the
    /// card manufacturer towards the ABC4Trust Lite application, so that only
    /// CryptoExperts can successfully execute this operation. A GET CHALLENGE
    /// command must be played before to provide a 16-byte random challenge
    /// from which a 16-byte authentication code (MAC) is computed
    /// and presented to the card.
    /// </summary>
    /// <param name="mac">Takes the result of result of the challenge process</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetVirginMode(byte[] mac);

    /// <summary>
    /// Returns a size-byte random number and memorizes it in the internal variable 
    /// BYTE[] challenge. Inherently limited to 256-byte random numbers.
    /// </summary>
    /// <param name="response">The response data as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetChallenge(int size, out byte[] response);

    /// <summary>
    /// Returns pin trials left on the card.
    /// </summary>
    /// <param name="pinTrialsLeft">The Trials left for PIN as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetPinTrials(out int pinTrialsLeft);

    /// <summary>
    /// Returns puk trials left on the card
    /// </summary>
    /// <param name="pukTrialsLeft">the trials left for PUK as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetPukTrials(out int pukTrialsLeft);

    /// <summary>
    /// If oldpin = PIN, replaces PIN with newpin
    /// </summary>
    /// <param name="oldPin">The old pid for the card</param>
    /// <param name="newPin">The new pid for the card</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetPin(byte[] oldPin, byte[] newPin);

    /// <summary>
    /// If puk = PUK, replaces PIN with pin,
    /// </summary>
    /// <param name="puk">puk for the card</param>
    /// <param name="pin">new pin for the card</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ResetPin(byte[] puk, byte[] pin);

    /// <summary>
    /// if pin = PIN, returns the available memory space left on the card in bytes. 
    /// Only works in root and working modes.
    /// </summary>
    /// <param name="pin">The pin for the card</param>
    /// <param name="memSpace">The space left on the card in bytes as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetMemorySpace(byte[] pin, out byte[] memSpace);
    
    /// <summary>
    /// Incoming and outgoing data (in the usual card-centric terminology) are transmitted
    /// back and forth between the card and the terminal application. The card allows
    /// write access to an internal input buer referred to as buffer. Arbitrary streams
    /// of bytes up to 2048 bytes can be written to buffer to provide data material to the card.
    /// 
    /// Provides the card with a stream of bytes i.e. sets buffer = datain. The data is ready
    /// to be used by the card in further commands.
    /// </summary>
    /// <param name="data">The data stream buffer</param>
    /// <returns>ErrorCode</returns>
    ErrorCode PutData(byte[] data);

    /// <summary>
    /// Provides the card with the identier of an authentication key. The command expects 
    /// buffer to hold an extended signature under the current value of
    /// challenge. The card will rst ascertain the validity of the signature with
    /// respect to the indicated authentication key prior to using the data. buffer
    /// is then replaced with the authenticated data extracted from the extended
    /// signature unless authentication fails. When authentication succeeds, a flag
    /// authdata is set to 1 and authkeyID memorizes the authentication key identier keyID.
    /// In case of authentication failure, authdata is set to 0. challenge
    /// is erased when this command is executed irrespective of success or failure.
    /// </summary>
    /// <param name="keyID">The keyID to be used</param>
    /// <returns>ErrorCode</returns>
    ErrorCode AuthData(byte keyID);

    /// <summary>
    /// sets the authentication key with identier keyID (or resets it if already defined)
    /// to the current value of buffer if the card is in root mode. If the 
    /// card is in working mode, it rst makes sure that authdata is set to 1, that 
    /// authkeyID = 0 and that the rst 2 bytes of buffer are equal to INS k keyID 
    /// where INS is a constant byte specic to the command (the remaining bytes then 
    /// serve as input data). In other words, a preliminary AUTHENTICATE DATA(0) 
    /// must have been played to provide a root-authenticated value for the new key. 
    /// If the provided key has less than 55 or more than #maxintsize = 512 bytes, 
    /// it is not memorized in the card and an error is returned.
    /// </summary>
    /// <param name="keyID">The keyID to be used</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetAuthKey(byte keyID);

    /// <summary>
    /// If pin = PIN, returns the concatenated identiers and sizes of all authentication keys
    /// available on the card.
    /// </summary>
    /// <param name="pin">The pin for the card</param>
    /// <param name="keys">A array of concatenations of (keyID || size(key)) over all available keys</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetAuthKeys(byte[] pin, out byte[] keys);

    /// <summary>
    /// If pin = PIN, returns the authentication key with identityer keyID.
    /// </summary>
    /// <param name="pin">The pin code for the card</param>
    /// <param name="keyID">key id</param>
    /// <param name="key">the auth data as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadAuthKey(byte[] pin, byte keyID, out byte[] key);

    /// <summary>
    /// removes the authentication key with identier keyID. If the card is in working mode,
    /// it first makes sure that authdata is set to 1, that authkeyID = 0 and that buffer 
    /// is identical to the 2-byte string (INS || keyID) where INS is a constant byte specific
    /// to the command.
    /// </summary>
    /// <param name="keyID">The ID of the key to remove</param>
    /// <returns>ErrorCode</returns>
    ErrorCode RemoveAuthKey(byte keyID);

    /// <summary>
    /// Calls INITIALIZE DEVICE on the smartcard with ID and card. Will return the reponse
    /// </summary>
    /// <param name="someID">ID of the N used</param>
    /// <param name="pin">size</param>
    /// <param name="response">response array. Need to decrypt reponse based on N</param>
    /// <returns>ErrorCode</returns>
    ErrorCode InitDevice(byte[] someID, byte[] size, out byte[] response);

    /// <summary>
    /// Populates the group with identier groupID with the current value of buffer
    /// according to the component type comptype:
    ///                                         0 indicates the modulus,
    ///                                         1 the group order,
    ///                                         2 the cofactor.
    /// If the group was previously undened, this implicitly creates a new group with
    /// identifier groupID.
    /// </summary>
    /// <param name="groupID">groupID to store the group under</param>
    /// <param name="comptype">Comptype</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetGroupComponent(byte groupID, int comptype);

    /// <summary>
    /// populates the group with identier groupID with a generator (a group element)
    /// with identifier genID
    /// </summary>
    /// <param name="groupID">GroupID to store the generator under</param>
    /// <param name="genID">Generator ID</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetGenerator(byte groupID, int genID);

    /// <summary>
    /// If pin = PIN, returns a byte array of identifiers of all the groups available 
    /// on the card.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="groupIDs">Array of groupIDs as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ListGroups(byte[] pin, out byte[] groupIDs);

    /// <summary>
    /// If pin = PIN, returns the information about the group.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="groupID">GroupID to lookup</param>
    /// <param name="sizeOfM">the size of m</param>
    /// <param name="sizeOfQ">the size of q</param>
    /// <param name="sizeOfF">the size of f</param>
    /// <param name="t">Number of generators assigned to the group</param>
    /// <returns></returns>
    ErrorCode ReadGroup(byte[] pin, byte groupID, out byte[] sizeOfM,
                        out byte[] sizeOfQ, out byte[] sizeOfF, out int t);

    /// <summary>
    /// if pin = PIN, returns the component according to the component type 
    /// comptype (0 indicates the modulus, 1 the group order, 2 the cofactor).
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="groupID">groupID for the component</param>
    /// <param name="comptype">component type</param>
    /// <param name="groupComp">data stream of the component.</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadGroupComponent(byte[] pin, byte groupID, int comptype, out byte[] groupComp);

    /// <summary>
    /// If pin = PIN, returns the generator with identifier genID on the specified group.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="groupID">groupID the generator is assigned</param>
    /// <param name="genID">Generator ID</param>
    /// <param name="generator">generator as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadGenerator(byte[] pin, byte groupID, int genID, out byte[] generator);

    /// <summary>
    /// Removes the group with identifier groupID
    /// </summary>
    /// <param name="groupID">ID of group to remove</param>
    /// <returns>ErrorCode</returns>
    ErrorCode RemoveGroup(byte groupID);

    /// <summary>
    /// creates a new counter (or resets an already created counter) and initializes 
    /// its components to the given values.
    /// </summary>
    /// <param name="counterID">ID for the counter</param>
    /// <param name="keyID">ID for the key</param>
    /// <param name="index">counter index</param>
    /// <param name="threshold">counter threshold</param>
    /// <param name="cursor">counter cursor</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetCounter(byte counterID, byte keyID, int index, int threshold, byte[] cursor);

    /// <summary>
    /// only works when the card is in working mode. The input data contains
    /// a public-key signature sig which is checked against the authentication key
    /// with identifier keyID.
    /// </summary>
    /// <param name="keyID">ID for the key</param>
    /// <param name="sig">public key signature</param>
    /// <returns>ErrorCode</returns>
    ErrorCode IncCounter(byte keyID, byte[] sig);

    /// <summary>
    /// If pin = PIN, returns the concatenated identifiers of all the counters
    /// available on the card.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="counters">array of counter IDs as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ListCounters(byte[] pin, out byte[] counters);

    /// <summary>
    /// If pin = PIN, returns the counter information assigned to counterID.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="counterID">ID for the counter</param>
    /// <param name="keyID">ID for the key</param>
    /// <param name="index">index for the counter</param>
    /// <param name="threshold">counter threshold</param>
    /// <param name="cursor">cursor used for the counter</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadCounter(byte[] pin, byte counterID, out byte keyID,
                          out int index, out int threshold, byte[] cursor);

    /// <summary>
    /// Eemoves the counter with identier counterID.
    /// </summary>
    /// <param name="counterID">ID of counter to remove</param>
    /// <returns>ErrorCode</returns>
    ErrorCode RemoveCounter(byte counterID);

    /// <summary>
    /// creates a new issuer (or resets an already created issuer) and initializes its components 
    /// to the given values.
    /// </summary>
    /// <param name="issuerID">ID for the issuer</param>
    /// <param name="groupID">group ID</param>
    /// <param name="genID1">???? lite doc not clean on what this is</param>
    /// <param name="genID2">???? lite doc not clean on what this is</param>
    /// <param name="numpres">????</param>
    /// <param name="counterID">counter ID</param>
    /// <returns></returns>
    ErrorCode SetIssuer(byte issuerID, byte groupID, byte genID1, byte genID2, byte numpres, byte counterID);

    /// <summary>
    /// If pin == PIN it returns the list of issuers ID.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="issuers">list of issuers IDs as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ListIssuers(byte[] pin, out byte[] issuers);

    /// <summary>
    /// if pin = PIN, returns information about the issuers
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="issuerID">ID for the issuer to look up</param>
    /// <param name="groupID">group ID for the issuer</param>
    /// <param name="genID1">gen ID 1 for the issuer</param>
    /// <param name="genID2">gen ID 2 for the issuer</param>
    /// <param name="numpres">numpres for the issuer</param>
    /// <param name="counterID">ID for the counter</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadIssuer(byte[] pin, byte issuerID, out byte groupID, out byte genID1, out byte genID2, out byte numpres, out byte counterID);

    /// <summary>
    /// Removes the issuer assigned with ID
    /// </summary>
    /// <param name="issuerID">ID of the issuer to remove</param>
    /// <returns>ErrorCode</returns>
    ErrorCode RemoveIssuer(byte issuerID);

    /// <summary>
    /// creates a new prover (or resets an already created prover) and initializes
    /// its components to the given values
    /// </summary>
    /// <param name="proverID">ID of the prover</param>
    /// <param name="kSize">size of k</param>
    /// <param name="cSize">size of c</param>
    /// <param name="credIds">array of credential IDs</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetProver(byte proverID, byte[] kSize, byte[] cSize, byte[] credIds);

    /// <summary>
    /// If pin == PIN it returns information about the proverID
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="proverID">ID of the prover</param>
    /// <param name="kSize">size of k</param>
    /// <param name="cSize">size of c</param>
    /// <param name="proofSession">proof session data</param>
    /// <param name="proofStatus">status of the proof</param>
    /// <param name="credIds">array of credential IDs</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadProver(byte[] pin, byte proverID, out byte[] kSize, out byte[] cSize,
                         out byte[] proofSession, out int proofStatus, out byte[] credIds);

    /// <summary>
    /// Remove the prover based on the ID
    /// </summary>
    /// <param name="proverID">ID on the prover to remove</param>
    /// <returns>ErrorCode</returns>
    ErrorCode RemoveProver(byte proverID);

    /// <summary>
    /// if pin = PIN, updates the prover with identier proverID with fresh, randomly 
    /// selected components
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="proverID">ID of the prover</param>
    /// <returns>ErrorCode</returns>
    ErrorCode BeginCommitments(byte[] pin, byte proverID, out byte[] outProofSession);

    void EndCommitments();

    /// <summary>
    /// If pin = PIN, proofstatus = 1 and currentproverID = proverID, fetches 
    /// proofsession from the prover proverID, computes the zero-knowledge challenge
    /// c from (proofsession; input) as described in Section 3.4.1, memorizes c
    /// and sets proofstatus = 2
    /// </summary>
    /// <param name="pin">pin of the card</param>
    /// <param name="proverID">ID of the prover</param>
    /// <param name="input">the input to use</param>
    /// <returns></returns>
    ErrorCode BeginResponse(byte[] pin, byte proverID, byte[] input);

    /// <summary>
    /// if pin = PIN, creates a new credential (or resets an already created credential) 
    /// and initializes its identier and issuer identifier to credentialID and issuerID respectively.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">ID for the credential</param>
    /// <param name="issuerID">issuer ID</param>
    /// <returns>ErrorCode</returns>
    ErrorCode SetCredential(byte[] pin, byte credID, byte issuerID);

    /// <summary>
    /// if pin = PIN, returns the concatenated identifiers of all the credentials
    /// available on the card.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credIDs">array for credential IDs</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ListCredentials(byte[] pin, out byte[] credIDs);

    /// <summary>
    /// If pin = PIN, returns information about the credential.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">credential ID to lookup</param>
    /// <param name="issuerID">ID for the issuer</param>
    /// <param name="vSize">size of V</param>
    /// <param name="kSize">size of V_k</param>
    /// <param name="status">status of credential</param>
    /// <param name="prescount">count</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadCredential(byte[] pin, byte credID, out byte issuerID, out byte[] vSize, out byte[] kSize, out int status, out byte prescount);

    /// <summary>
    /// If pin = PIN it remove the credential
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">ID for the credential to remove</param>
    /// <returns>ErrorCode</returns>
    ErrorCode RemoveCredential(byte[] pin, byte credID);

    /// <summary>
    /// if pin = PIN, returns pubKey as described in Section 3.5.1. This does not 
    /// change the status eld of the credential.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">credential ID</param>
    /// <param name="pubKey">the public key as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetCredentialPublicKey(byte[] pin, byte credID, out byte[] pubKey);

    /// <summary>
    /// if pin = PIN, status = 0, the credential is a member of the current proof 
    /// session and the current proof session is in commitment stage, returns C as
    /// described in Section 3.5.1 and sets status = 1.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">ID of the credential</param>
    /// <param name="C">C as an out param</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetIssuanceCommitment(byte[] pin, byte credID, out byte[] C);

    /// <summary>
    /// if pin = PIN, status = 1, the credential is a member of the current proof 
    /// session and the current proof session is in response stage, returns the response 
    /// R as described in Section 3.5.1 and sets status = 2.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">ID for the credential</param>
    /// <param name="R">R</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetIssuanceResponse(byte[] pin, byte credID, out byte[] R);

    /// <summary>
    /// if pin = PIN, status = 2, the credential is a member of the current proof
    /// session, the current proof session is in commitment stage and if no immature
    /// counter is attached to the credential's issuer, returns C as described in
    /// Section 3.5.1 and sets status = 3.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">credential ID</param>
    /// <param name="C">C</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetPresentationCommitment(byte[] pin, byte credID, out byte[] C);

    /// <summary>
    /// if pin = PIN, status = 3, the credential is a member of the current proof
    /// session and the current proof session is in response stage, returns the response
    /// R as described in Section 3.5.1. prescount is incremented and if prescount 
    /// numpres 6= 0 then status = 4, otherwise status = 2.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="credID">credential ID</param>
    /// <param name="R">R</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetPresentationResponse(byte[] pin, byte credID, out byte[] R);

    /// <summary>
    /// if pin = PIN, returns g^x mod m where x = deviceKey.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="pubKey">device public key</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetDevicePublicKey(byte[] pin, out byte[] pubKey);

    /// <summary>
    /// if pin = PIN, the current proof session includes pseudonyms and is in commitment stage.
    /// a_d
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="C">C</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetDeviceCommitment(byte[] pin, out byte[] C);

    /// <summary>
    /// If pin = PIN, the current proof session includes pseudonyms and is in response stage.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="R">R</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetDeviceResponse(byte[] pin, out byte[] R);


    /// <summary>
    /// If pin = PIN, returns (h(scope)^f)^x mod m where x = deviceKey, f = is a group param (co-factor).
    /// page 19 in u-prove spec 
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="scope">scope</param>
    /// <param name="pseudonym">pseudonym</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetScopeExlusivePseudonym(byte[] pin, byte[] scope, out byte[] pseudonym);


    /// <summary>
    /// If pin = PIN, the current proof session includes pseudonyms and is in commitment stage.
    /// 
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="scope">scope</param>
    /// <param name="C">C</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetScopeExlusiveCommitment(byte[] pin, byte[] scope, out byte[] C);


    /// <summary>
    /// If pin = PIN, the current proof session includes pseudonyms and is in response stage.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="scope">scope</param>
    /// <param name="R">R</param>
    /// <returns>ErrorCode</returns>
    ErrorCode GetScopeExlusiveResponse(byte[] pin, byte[] scope, out byte[] R);

    /// <summary>
    /// If pin = PIN, stores the blob (uri; buffer) in the BlobStore. This rewrites 
    /// any pre-existing blob with identical URI field.
    /// </summary>
    /// <param name="pin">Pin for the card</param>
    /// <param name="uri">The byte array to store</param>
    /// <returns>ErrorCode</returns>
    ErrorCode StoreBlob(byte[] pin, byte[] uri);


    /// <summary>
    /// Get
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="nreads"></param>
    /// <returns></returns>
    //ErrorCode ListBlobs(byte[] pin, byte nreads, out byte[] blobs);


    /// <summary>
    /// If pin = PIN, returns the data eld of the blob which URI field equals uri.
    /// Returns an error if the blob is unfound.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="uri">uri on the blob to read</param>
    /// <param name="blob">blob data</param>
    /// <returns>ErrorCode</returns>
    ErrorCode ReadBlob(byte[] pin, byte[] uri, out byte[] blob);

    /// <summary>
    /// If pin = PIN, it removes the blob with the assigned uri.
    /// </summary>
    /// <param name="pin">pin for the card</param>
    /// <param name="uri">uri for the blob to remove</param>
    /// <returns>ErrorCode</returns>
    ErrorCode RemoveBlob(byte[] pin, byte[] uri);

  }
}
