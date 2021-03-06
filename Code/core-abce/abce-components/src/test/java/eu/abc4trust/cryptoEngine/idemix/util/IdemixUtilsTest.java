//* Licensed Materials - Property of IBM, Miracle A/S, and            *
//* Alexandra Instituttet A/S                                         *
//* eu.abc4trust.pabce.1.0                                            *
//* (C) Copyright IBM Corp. 2012. All Rights Reserved.                *
//* (C) Copyright Miracle A/S, Denmark. 2012. All Rights Reserved.    *
//* (C) Copyright Alexandra Instituttet A/S, Denmark. 2012. All       *
//* Rights Reserved.                                                  *
//* US Government Users Restricted Rights - Use, duplication or       *
//* disclosure restricted by GSA ADP Schedule Contract with IBM Corp. *
//*                                                                   *
//* This file is licensed under the Apache License, Version 2.0 (the  *
//* "License"); you may not use this file except in compliance with   *
//* the License. You may obtain a copy of the License at:             *
//*   http://www.apache.org/licenses/LICENSE-2.0                      *
//* Unless required by applicable law or agreed to in writing,        *
//* software distributed under the License is distributed on an       *
//* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY            *
//* KIND, either express or implied.  See the License for the         *
//* specific language governing permissions and limitations           *
//* under the License.                                                *
//*/**/****************************************************************

package eu.abc4trust.cryptoEngine.idemix.util;


import static org.junit.Assert.assertTrue;

import org.junit.Test;

import com.ibm.zurich.idmx.dm.structure.CredentialStructure;

import eu.abc4trust.xml.CredentialSpecification;
import eu.abc4trust.xml.util.XmlUtils;

public class IdemixUtilsTest{
  
  @Test
  public void testIdemixUtilsGenerateCredStructForOneOf() throws Exception {		
	  	 // Step 1. Load credspec from XML.
	     CredentialSpecification creditCardSpec =
	                (CredentialSpecification) XmlUtils.getObjectFromXML(getClass().getResourceAsStream(
	                		"/eu/abc4trust/sampleXml/soderhamn/credentialSpecificationSoderhamnClassOneOf.xml"), true);  
	     
	     // Step 2. Generate the credential structure
	     CredentialStructure cs = IdemixUtils.createIdemixCredentialStructure(creditCardSpec);
	     
	     assertTrue(cs!=null);
  }
  

}
