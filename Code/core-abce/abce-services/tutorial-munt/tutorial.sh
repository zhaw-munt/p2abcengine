#!/bin/bash

#Stop script if an error occurs.
set -e
# Setup System Parameters.
echo "Setup System Parameters"
curl -X POST --header 'Content-Type: text/xml' 'http://localhost:9100/issuer/setupSystemParameters/?securityLevel=80&cryptoMechanism=urn:abc4trust:1.0:algorithm:idemix' > ./out/systemparameters.xml

# Store credential specification at issuer.
echo "Store credential specification at issuer"
curl -X PUT --header 'Content-Type: text/xml' -d @./credentialSpecificationRoomReservation.xml 'http://localhost:9100/issuer/storeCredentialSpecification/http%3A%2F%2Fmroman.ch%2FRoomReservation%2Fspec' > ./out/storeCredentialSpecificationAtIssuerResponce.xml
cat ./out/storeCredentialSpecificationAtIssuerResponce.xml

# Store credential specification at user.
# This method is not specified in H2.2.
echo "Store credential specification at user"
curl -X PUT --header 'Content-Type: text/xml' -d @./credentialSpecificationRoomReservation.xml 'http://localhost:9200/user/storeCredentialSpecification/http%3A%2F%2Fmroman.ch%2FRoomReservation%2Fspec' > ./out/storeCredentialSpecificationAtUserResponce.xml
cat ./out/storeCredentialSpecificationAtUserResponce.xml

# Store credential specification at verifier.
# This method is not specified in H2.2.
echo "Store credential specification at verifier"
curl -X PUT --header 'Content-Type: text/xml' -d @./credentialSpecificationRoomReservation.xml 'http://localhost:9300/verification/storeCredentialSpecification/http%3A%2F%2Fmroman.ch%2FRoomReservation%2Fspec' > ./out/storeCredentialSpecificationAtVerifierResponce.xml
cat ./out/storeCredentialSpecificationAtVerifierResponce.xml

echo "!! Credential Spec should now have been stored at Issuer, User and Verifier !!!"
#read -p "Press [Enter] to continue.."

# Store System parameters at Revocation Authority.
# This method is not specified in H2.2.
echo "Store System parameters at Revocation Authority"
curl -X POST --header 'Content-Type: text/xml' -d @./out/systemparameters.xml 'http://localhost:9500/revocation/storeSystemParameters/' > ./out/storeSystemParametersResponceAtRevocationAutority.xml

# Store System parameters at User.
# This method is not specified in H2.2.
echo "Store System parameters at User"
curl -X POST --header 'Content-Type: text/xml' -d @./out/systemparameters.xml 'http://localhost:9200/user/storeSystemParameters/' > ./out/storeSystemParametersResponceAtUser.xml

# Store System parameters at verifier.
# This method is not specified in H2.2.
echo "Store System parameters at Verifier"
curl -X POST --header 'Content-Type: text/xml' -d @./out/systemparameters.xml 'http://localhost:9300/verification/storeSystemParameters/' > ./out/storeSystemParametersResponceAtVerifier.xml


# Setup Revocation Authority Parameters.
echo "Setup Revocation Authority Parameters"
curl -X POST --header 'Content-Type: text/xml' -d @./revocationReferences.xml 'http://localhost:9500/revocation/setupRevocationAuthorityParameters?keyLength=1024&cryptoMechanism=urn:abc4trust:1.0:algorithm:idemix&uid=http%3A%2F%2Fticketcompany%2Frevocation' > ./out/revocationAuthorityParameters.xml

# Store Revocation Authority Parameters at issuer.
# This method is not specified in H2.2.
echo "Store Revocation Authority Parameters at issuer"
curl -X PUT --header 'Content-Type: text/xml' -d @./out/revocationAuthorityParameters.xml 'http://localhost:9100/issuer/storeRevocationAuthorityParameters/http%3A%2F%2Ftmroman.ch%2FRoomReservation%2Frevocation'  > ./out/storeRevocationAuthorityParameters.xml

# Store Revocation Authority Parameters at user.
# This method is not specified in H2.2.
echo "Store Revocation Authority Parameters at user"
curl -X PUT --header 'Content-Type: text/xml' -d @./out/revocationAuthorityParameters.xml 'http://localhost:9200/user/storeRevocationAuthorityParameters/http%3A%2F%2Ftmroman.ch%2FRoomReservation%2Frevocation'  > ./out/storeRevocationAuthorityParametersAtUserResponce.xml

# Store Revocation Authority Parameters at verifier.
# This method is not specified in H2.2.
echo "Store Revocation Authority Parameters at verifier"
curl -X PUT --header 'Content-Type: text/xml' -d @./out/revocationAuthorityParameters.xml 'http://localhost:9300/verification/storeRevocationAuthorityParameters/http%3A%2F%2Ftmroman.ch%2FRoomReservation%2Frevocation'  > ./out/storeRevocationAuthorityParametersAtVerifierResponce.xml


# Setup issuer parameters.
echo "Setup issuer parameters"
curl -X POST --header 'Content-Type: text/xml' -d @./issuerParametersInput.xml 'http://localhost:9100/issuer/setupIssuerParameters/' > ./out/issuerParameters.xml


# Store Issuer Parameters at user.
# This method is not specified in H2.2.
echo "Store Issuer Parameters at user"
curl -X PUT --header 'Content-Type: text/xml' -d @./out/issuerParameters.xml 'http://localhost:9200/user/storeIssuerParameters/http%3A%2F%2Fmroman.ch%2FRoomReservation%2Fissuance%3Aidemix'  > ./out/storeIssuerParametersAtUser.xml

# Store Issuer Parameters at verifier.
# This method is not specified in H2.2.
echo "Store Issuer Parameters at verifier"
curl -X PUT --header 'Content-Type: text/xml' -d @./out/issuerParameters.xml 'http://localhost:9300/verification/storeIssuerParameters/http%3A%2F%2Fmroman.ch%2FRoomReservation%2Fissuance%3Aidemix'  > ./out/storeIssuerParametersAtVerifier.xml

# Create smartcard at user.
# This method is not specified in H2.2.
echo "Create smartcard at user"
curl -X POST --header 'Content-Type: text/xml' 'http://localhost:9200/user/createSmartcard/http%3A%2F%2Fmroman.ch%2FRoomReservation%2Fissuance%3Aidemix'

echo "!!! Issuer Parameters should now have been set !!!"
#read -p "Press [Enter] to continue..."

# Init issuance protocol (first step for the issuer).
echo "Init issuance protocol"
curl -X POST --header 'Content-Type: text/xml' -d @./issuancePolicyAndAttributes.xml 'http://localhost:9100/issuer/initIssuanceProtocol/' > ./out/issuanceMessageAndBoolean.xml

# Extract issuance message.
curl -X POST --header 'Content-Type: text/xml' -d @./out/issuanceMessageAndBoolean.xml 'http://localhost:9200/user/extractIssuanceMessage/' > ./out/firstIssuanceMessage.xml

# First issuance protocol step (first step for the user).
echo "First issuance protocol step for the user"
curl -X POST --header 'Content-Type: text/xml' -d @./out/firstIssuanceMessage.xml 'http://localhost:9200/user/issuanceProtocolStep/' > ./out/issuanceReturn.xml

# Setup uiIssuanceReturn.xml.
UiContext=`cat ./out/issuanceReturn.xml | sed 's/^.*<uiContext>//' | sed 's/<\/uiContext>.*//'`
# echo ${UiContext}
cat ./uiIssuanceReturn.xml | sed "s#REPLACE-THIS-CONTEXT#${UiContext}#" > ./out/uiIssuanceReturn.xml

# First issuance protocol step - UI (first step for the user).
echo "Second issuance protocol step (first step for the user)"
curl -X POST --header 'Content-Type: text/xml' -d @./out/uiIssuanceReturn.xml 'http://localhost:9200/user/issuanceProtocolStepUi/' > ./out/secondIssuanceMessage.xml

# Second issuance protocol step (second step for the issuer).
echo "Second issuance protocol step (second step for the issuer)"
curl -X POST --header 'Content-Type: text/xml' -d @./out/secondIssuanceMessage.xml 'http://localhost:9100/issuer/issuanceProtocolStep/' > ./out/thirdIssuanceMessageAndBoolean.xml

# Extract issuance message.
curl -X POST --header 'Content-Type: text/xml' -d @./out/thirdIssuanceMessageAndBoolean.xml 'http://localhost:9200/user/extractIssuanceMessage/' > ./out/thirdIssuanceMessage.xml

# Third issuance protocol step (second step for the user).
echo "Third issuance protocol step (second step for the user)"
curl -X POST --header 'Content-Type: text/xml' -d @./out/thirdIssuanceMessage.xml 'http://localhost:9200/user/issuanceProtocolStep/' > ./out/fourthIssuanceMessageAndBoolean.xml

# Create presentation policy alternatives.
# This method is not specified in H2.2.
echo "Create presentation policy alternatives"
curl -X GET --header 'Content-Type: text/xml' -d @./presentationPolicyAlternatives.xml 'http://localhost:9300/verification/createPresentationPolicy/' > ./out/presentationPolicyAlternatives.xml

# Create presentation UI return.
# This method is not specified in H2.2.
echo "Create presentation UI return"
curl -X POST --header 'Content-Type: text/xml' -d @./out/presentationPolicyAlternatives.xml 'http://localhost:9200/user/createPresentationToken/' > ./out/presentationReturn.xml

# Setup uiPresentationReturn.xml.
UiContext=`cat ./out/presentationReturn.xml | sed 's/^.*<uiContext>//' | sed 's/<\/uiContext>.*//'`
# echo ${UiContext}
cat ./uiPresentationReturn.xml | sed "s#REPLACE-THIS-CONTEXT#${UiContext}#" > ./out/uiPresentationReturn.xml
  
# Create presentation token.
# This method is not specified in H2.2.
echo "Create presentation token"
curl -X POST --header 'Content-Type: text/xml' -d @./out/uiPresentationReturn.xml 'http://localhost:9200/user/createPresentationTokenUi/' > ./out/presentationToken.xml

# Setup presentationPolicyAlternativesAndPresentationToken.xml.
presentationPolicy=`cat ./out/presentationPolicyAlternatives.xml | sed 's/^.*<PresentationPolicyAlternatives xmlns="http:\/\/abc4trust.eu\/wp2\/abcschemav1.0" Version="1.0">//' | sed 's/<\/PresentationPolicyAlternatives>.*//'`
presentationToken=`cat ./out/presentationToken.xml | sed 's/^.*<PresentationToken xmlns="http:\/\/abc4trust.eu\/wp2\/abcschemav1.0" Version="1.0">//' | sed 's/<\/PresentationToken>.*//'`
# echo "${presentationPolicy}"
# echo "${presentationToken}"
echo '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>' > ./out/presentationPolicyAlternativesAndPresentationToken.xml
echo '<PresentationPolicyAlternativesAndPresentationToken xmlns="http://abc4trust.eu/wp2/abcschemav1.0" Version="1.0"> <PresentationPolicyAlternatives>' >> ./out/presentationPolicyAlternativesAndPresentationToken.xml
echo "${presentationPolicy}" >> ./out/presentationPolicyAlternativesAndPresentationToken.xml
echo '</PresentationPolicyAlternatives>' >> ./out/presentationPolicyAlternativesAndPresentationToken.xml
echo '<PresentationToken>' >> ./out/presentationPolicyAlternativesAndPresentationToken.xml
echo "${presentationToken}" >> ./out/presentationPolicyAlternativesAndPresentationToken.xml
echo '</PresentationToken>' >> ./out/presentationPolicyAlternativesAndPresentationToken.xml
echo '</PresentationPolicyAlternativesAndPresentationToken>' >> ./out/presentationPolicyAlternativesAndPresentationToken.xml
  
# Verify presentation token against presentation policy.
echo "Verify presentation token against presentation policy"
# This method is not specified in H2.2.
curl -X POST --header 'Content-Type: text/xml' -d @./out/presentationPolicyAlternativesAndPresentationToken.xml 'http://localhost:9300/verification/verifyTokenAgainstPolicy/' > ./out/presentationTokenDescription.xml


