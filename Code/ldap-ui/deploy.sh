#!/bin/sh

mvn clean install
~/web/web1/bin/shutdown.sh
rm -rf ~/web/web1/webapps/ldap-ui-service/
cp -f ./target/ldap-ui-service.war ~/web/web1/webapps/ldap-ui-service.war
cp -rf ./target/ldap-ui-service ~/web/web1/webapps/
~/web/web1/bin/startup.sh
