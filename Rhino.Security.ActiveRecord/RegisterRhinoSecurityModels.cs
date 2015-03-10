using System;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Framework.Internal;
using NHibernate;
using NHibernate.Cfg;
namespace Rhino.Security.ActiveRecord
{
	public class RegisterRhinoSecurityModels
	{
		private readonly ActiveRecordModelBuilder modelBuilder = new ActiveRecordModelBuilder();

        public void BeforeInitialization()
        {
            ActiveRecordStarter.ModelsValidated += delegate
            {
                foreach (Type type in RhinoSecurity.Entities)
                {
                    modelBuilder.CreateDummyModelFor(type);
                }
            };
        }

		public void Configured(Configuration cfg)
		{
		}

		public void Initialized(Configuration cfg, ISessionFactory sessionFactory)
		{
		}
	}
}