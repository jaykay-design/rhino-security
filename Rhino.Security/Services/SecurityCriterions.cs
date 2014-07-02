using System;

namespace Rhino.Security.Services
{
	using Model;
	using NHibernate.Criterion;
	using NHibernate.SqlCommand;

	/// <summary>
	/// A factory for common DetachedCriteria.
	/// </summary>
	internal class SecurityCriterions
	{
		public static DetachedCriteria DirectUsersGroups(IUser user)
		{
			return DetachedCriteria.For<UsersGroup>()
				.CreateAlias("Users", "user")
				.Add(Expression.Eq("user.id", user.SecurityInfo.Identifier));
		}

        public static DetachedCriteria DirectEntitiesGroups<TEntity>(TEntity entity) where TEntity : class
        {
            Guid key = Security.ExtractKey(entity);
            return DetachedCriteria.For<EntitiesGroup>()
                .CreateAlias("Entities", "e")
                .Add(Expression.Eq("e.EntitySecurityKey", key));
        }

		public static DetachedCriteria AllGroups(IUser user)
		{
            return DetachedCriteria.For<UsersGroup>()
                .Add(Expression.Sql(
                "{alias}.Id IN (SELECT GroupId FROM {{UsersToUsersGroupsInherited}} WHERE UserId = ?)".PrefixTableName(),
                new object[] { user.SecurityInfo.Identifier },
                new NHibernate.Type.IType[] { NHibernate.NHibernateUtil.Guid }));
		}
        
        public static DetachedCriteria AllGroups<TEntity>(TEntity entity)where TEntity:class
        {
            Guid key = Security.ExtractKey(entity);

            return DetachedCriteria.For<EntitiesGroup>()
                .Add(Expression.Sql(
@"{alias}.Id IN (
SELECT ParentGroup FROM {{EntityGroupsHierarchy}} WHERE ChildGroup IN
    (
        SELECT 
            e2eg.GroupId 
        FROM 
            {{EntityReferences}} er JOIN
            {{EntityReferencesToEntitiesGroups}} e2eg ON er.Id = e2eg.EntityReferenceId
        WHERE 
            er.EntitySecurityKey = ?
    )

UNION 
        SELECT 
            e2eg.GroupId 
        FROM 
            {{EntityReferences}} er JOIN
            {{EntityReferencesToEntitiesGroups}} e2eg ON er.Id = e2eg.EntityReferenceId
        WHERE 
            er.EntitySecurityKey = ?
    )
".PrefixTableName(),
                new object[] { key, key },
                new NHibernate.Type.IType[] { NHibernate.NHibernateUtil.Guid, NHibernate.NHibernateUtil.Guid }));
            
        }
		
	}
}
