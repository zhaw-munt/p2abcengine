package ch.zhaw.ficore.p2abc.services.issuance;

import org.slf4j.LoggerFactory;
import org.slf4j.ext.XLogger;

import ch.zhaw.ficore.p2abc.configuration.IssuanceConfiguration;
import ch.zhaw.ficore.p2abc.xml.AttributeInfoCollection;

/**
 * Serves as a Factory for AttributeInfoProviders.
 * 
 * An AttributeInfoProvider is capable of providing meta-information about
 * Attributes from an identity source. This meta-information is required to
 * create corresponding CredentialSpecifications.
 * 
 * @author mroman
 */
public abstract class AttributeInfoProvider {
    private static final XLogger logger = new XLogger(
            LoggerFactory.getLogger(AttributeInfoProvider.class));

    protected IssuanceConfiguration configuration;

    public AttributeInfoProvider(final IssuanceConfiguration configuration) {
        this.configuration = configuration;
    }

    /**
     * Factory method to construct an AttributeInfoProvider for a given
     * ServiceConfiguration. The AttributeInfoProvider will receive a reference
     * to the configuration.
     * 
     * @param configuration
     *            Configuration
     * @return an implementation of an AttributeInfoProvider
     */
    public static AttributeInfoProvider getAttributeInfoProvider(
            final IssuanceConfiguration configuration) {
        logger.entry();

        switch (configuration.getAttributeSource()) {
        case FAKE:
            return logger.exit(new FakeAttributeInfoProvider(configuration));
        case LDAP:
            return logger.exit(new LdapAttributeInfoProvider(configuration));
        case JDBC:
            return logger.exit(new JdbcAttributeInfoProvider(configuration));
        default:
            logger.error("Identity source "
                    + configuration.getAttributeSource() + " not supported");
            return logger.exit(null);
        }
    }

    /**
     * Called when this AttributeInfoProvider is no longer required. Providers
     * should close open sockets/connections/files etc. on shutdown.
     */
    public abstract void shutdown();

    /**
     * Returns the collected meta-information about attributes of <em>name</em>.
     * The <em>name</em> may and should be used to distinguish between different
     * objects/credentials a user can obtain. I.e. <em>name</em> may refer to an
     * objectClass in an LDAP identity source.
     * 
     * The exact behaviour of <em>name</em> is provider specific.
     * 
     * @param name
     *            <em>name</em>
     * @return An AttributeInfoCollection
     */
    public abstract AttributeInfoCollection getAttributes(String name);
}
