using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Rhino.Security.Impl;
using Rhino.Security.Impl.Util;
using Rhino.Security.Interfaces;
using Rhino.Security.Model;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rhino.Security.Services
{
	/// <summary>
	/// Allows to edit the security information of the 
	/// system
	/// </summary>
	public class AuthorizationRepository : IAuthorizationRepository
	{
	    private readonly ISession session;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationRepository"/> class.
		/// </summary>
		public AuthorizationRepository(ISession session)
		{
		    this.session = session;
		}

	    #region IAuthorizationRepository Members

		/// <summary>
		/// Creates a new users group.
		/// </summary>
		/// <param name="name">The name of the new group.</param>
		public virtual UsersGroup CreateUsersGroup(string name)
		{
			var ug = new UsersGroup {Name = name};
		    session.Save(ug);
			return ug;
		}

		/// <summary>
		/// Creates the users group as a child of <paramref name="parentGroupName"/>.
		/// </summary>
		/// <param name="parentGroupName">Name of the parent group.</param>
		/// <param name="usersGroupName">Name of the users group.</param>
		/// <returns></returns>
		public virtual UsersGroup CreateChildUserGroupOf(string parentGroupName, string usersGroupName)
		{
			UsersGroup parent = GetUsersGroupByName(parentGroupName);
			Guard.Against<ArgumentException>(parent == null,
			                                 "Parent users group '" + parentGroupName + "' does not exists");

			UsersGroup group = CreateUsersGroup(usersGroupName);
			group.Parent = parent;
			group.AllParents.AddAll(parent.AllParents);
			group.AllParents.Add(parent);
			parent.DirectChildren.Add(group);

            foreach (var p in group.AllParents)
            {
                p.AllChildren.Add(group);
            }

			return group;
		}

        /// <summary>
        /// Creates the entity group as a child of <paramref name="parentGroupName"/>.
        /// </summary>
        /// <param name="parentGroupName">Name of the parent group.</param>
        /// <param name="usersGroupName">Name of the users group.</param>
        /// <returns></returns>
        public virtual EntitiesGroup CreateChildEntityGroupOf(string parentGroupName, string usersGroupName)
        {
            EntitiesGroup parent = GetEntitiesGroupByName(parentGroupName);
            Guard.Against<ArgumentException>(parent == null, 
                                            "Parent users group '" + parentGroupName + "' does not exists");
            EntitiesGroup group = CreateEntitiesGroup(usersGroupName);
            group.Parent = parent;
            group.AllParents.AddAll(parent.AllParents);
            group.AllParents.Add(parent);
            parent.DirectChildren.Add(group);

            foreach (var p in group.AllParents)
            {
                p.AllChildren.Add(group);
            }

            return group;
        }

		/// <summary>
		/// temporary string
		/// </summary>
		/// <param name="usersGroupName">Name of the users group.</param>
		public virtual void RemoveUsersGroup(string usersGroupName)
		{
			UsersGroup group = GetUsersGroupByName(usersGroupName);
			if (group == null)
				return;

			Guard.Against(group.DirectChildren.Count != 0, "Cannot remove users group '"+usersGroupName+"' because is has child groups. Remove those groups and try again.");

		    session.CreateQuery("DELETE Permission p WHERE p.UsersGroup = :group")
		        .SetEntity("group", group)
		        .ExecuteUpdate();

			// we have to do this in order to ensure that we play
			// nicely with the second level cache and collection removals
			if (group.Parent!=null)
			{
				group.Parent.DirectChildren.Remove(group);
			}
			foreach (UsersGroup parent in group.AllParents)
			{
				parent.AllChildren.Remove(group);
			}
			group.AllParents.Clear();
			group.Users.Clear();

			session.Delete(group);
		}

        ///<summary>
        /// Renames an existing users group
        ///</summary>
        ///<param name="usersGroupName">The name of the usersgroup to rename</param>
        ///<param name="newName">The new name of the usersgroup</param>
        ///<returns>The renamed group</returns>       
        public virtual UsersGroup RenameUsersGroup(string usersGroupName, string newName)
        {
            UsersGroup group = GetUsersGroupByName(usersGroupName);
            Guard.Against(group == null, "There is no users group named: " + usersGroupName);
            group.Name = newName;
            
            session.Save(group);
            return group;
        }


		/// <summary>
		/// Removes the specified entities group.
		/// Will also delete all permissions that are associated with this group.
		/// </summary>
		/// <param name="entitesGroupName">Name of the entites group.</param>
		public virtual void RemoveEntitiesGroup(string entitesGroupName)
		{
			EntitiesGroup group = GetEntitiesGroupByName(entitesGroupName);
			if(group==null)
				return;
            Guard.Against(group.DirectChildren.Count != 0, "Cannot remove entity group '" + entitesGroupName + "' because is has child groups. Remove those groups and try again.");

		    session.CreateQuery("delete Permission p where p.EntitiesGroup = :group")
		        .SetEntity("group", group)
		        .ExecuteUpdate();

            // we have to do this in order to ensure that we play
            // nicely with the second level cache and collection removals
            if (group.Parent != null)
		    {
		        group.Parent.DirectChildren.Remove(group);
		    }
		    foreach (EntitiesGroup parent in group.AllParents)
		    {
		        parent.AllChildren.Remove(group);
		    }
            group.AllParents.Clear();
            group.Entities.Clear();

			session.Delete(group);
		}


		/// <summary>
		/// Removes the specified operation.
		/// Will also delete all permissions for this operation
		/// </summary>
		/// <param name="operationName">The operation N ame.</param>
		public virtual void RemoveOperation(string operationName)
		{
			Operation operation = GetOperationByName(operationName);
			if(operation==null)
				return;

			Guard.Against(operation.Children.Count != 0, "Cannot remove operation '"+operationName+"' because it has child operations. Remove those operations and try again.");

            session.CreateQuery("delete Permission p where p.Operation = :operation")
                .SetEntity("operation", operation)
                .ExecuteUpdate();

			// so we can play safely with the 2nd level cache & collections
			if(operation.Parent!=null)
			{
				operation.Parent.Children.Remove(operation);
			}

			session.Delete(operation);
		}

		/// <summary>
		/// Gets the ancestry association of a user with the named users group.
		/// This allows to track how a user is associated to a group through 
		/// their ancestry.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="usersGroupName">Name of the users group.</param>
		/// <returns></returns>
		public virtual UsersGroup[] GetAncestryAssociation(IUser user, string usersGroupName)
		{
			UsersGroup desiredGroup = GetUsersGroupByName(usersGroupName);
		    ICollection<UsersGroup> directGroups =
		        SecurityCriterions.DirectUsersGroups((user))
		            .GetExecutableCriteria(session)
                    .SetCacheable(true)
		            .List<UsersGroup>();

			if (directGroups.Contains(desiredGroup))
			{
				return new[] {desiredGroup};
			}
			// as a nice benefit, this does an eager load of all the groups in the hierarchy
			// in an efficient way, so we don't have SELECT N + 1 here, nor do we need
			// to load the Users collection (which may be very large) to check if we are associated
			// directly or not
			UsersGroup[] associatedGroups = GetAssociatedUsersGroupFor(user);
			if (Array.IndexOf(associatedGroups, desiredGroup) == -1)
			{
				return new UsersGroup[0];
			}
			// now we need to find out the path to it
			List<UsersGroup> shortest = new List<UsersGroup>();
			foreach (UsersGroup usersGroup in associatedGroups)
			{
				List<UsersGroup> path = new List<UsersGroup>();
				UsersGroup current = usersGroup;
				while (current != null && current != desiredGroup)
				{
					path.Add(current);
					current = current.Parent;
				}
				if (current != null)
					path.Add(current);
				// Valid paths are those that are contains the desired group
				// and start in one of the groups that are directly associated
				// with the user
				if (path.Contains(desiredGroup) && directGroups.Contains(path[0]))
				{
					shortest = Min(shortest, path);
				}
			}
			return shortest.ToArray();
		}
        /// <summary>
        /// Gets the ancestry association of an entity with the named entity group.
        /// This allows to track how an entity is associated to a group through 
        /// their ancestry.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityGroupName">Name of the entity group.</param>
        /// <returns></returns>
        public virtual EntitiesGroup[] GetAncestryAssociationOfEntity<TEntity>(TEntity entity, string entityGroupName) where TEntity : class
        {
            EntitiesGroup desiredGroup = GetEntitiesGroupByName(entityGroupName);
            ICollection<EntitiesGroup> directGroups =
                SecurityCriterions.DirectEntitiesGroups(entity)
                    .GetExecutableCriteria(session)
                    .SetCacheable(true)
                    .List<EntitiesGroup>();

            if (directGroups.Contains(desiredGroup))
            {
                return new[] { desiredGroup };
            }
            // as a nice benefit, this does an eager load of all the groups in the hierarchy
            // in an efficient way, so we don't have SELECT N + 1 here, nor do we need
            // to load the Entities collection (which may be very large) to check if we are associated
            // directly or not
            EntitiesGroup[] associatedGroups = GetAssociatedEntitiesGroupsFor(entity);
            if (Array.IndexOf(associatedGroups, desiredGroup) == -1)
            {
                return new EntitiesGroup[0];
            }
            // now we need to find out the path to it
            List<EntitiesGroup> shortest = new List<EntitiesGroup>();
            foreach (EntitiesGroup entitiesGroup in associatedGroups)
            {
                List<EntitiesGroup> path = new List<EntitiesGroup>();
                EntitiesGroup current = entitiesGroup;
                while (current != null && current != desiredGroup)
                {
                    path.Add(current);
                    current = current.Parent;
                }
                if (current != null)
                    path.Add(current);
                // Valid paths are those that are contains the desired group
                // and start in one of the groups that are directly associated
                // with the user
                if (path.Contains(desiredGroup) && directGroups.Contains(path[0]))
                {
                    shortest = Min(shortest, path);
                }
            }
            return shortest.ToArray();
        }

		/// <summary>
		/// Creates a new entities group.
		/// </summary>
		/// <param name="name">The name of the new group.</param>
		public virtual EntitiesGroup CreateEntitiesGroup(string name)
		{
			EntitiesGroup eg = new EntitiesGroup {Name = name};
		    session.Save(eg);
			return eg;
		}

        ///<summary>
        /// Renames an existing entities group
        ///</summary>
        ///<param name="entitiesGroupName">The name of the entities group to rename</param>
        ///<param name="newName">The new name of the entities group</param>
        ///<returns>The renamed group</returns>       
        public virtual EntitiesGroup RenameEntitiesGroup(string entitiesGroupName, string newName)
        {
            EntitiesGroup group = GetEntitiesGroupByName(entitiesGroupName);
            Guard.Against(group == null, "There is no entities group named: " + entitiesGroupName);
            group.Name = newName;
            session.Save(group);
            return group;
        }

		/// <summary>
		/// Gets the associated users group for the specified user.
		/// </summary>
		/// <param name="user">The user.</param>
		public virtual UsersGroup[] GetAssociatedUsersGroupFor(IUser user)
		{
            return session
                .CreateQuery(@"
                    SELECT ug
                    FROM 
                        UsersGroup ug JOIN ug.Users u WITH u.id = :user
                    ORDER BY ug.Name
                ")
                .SetParameter("user", user.SecurityInfo.Identifier)
                .List<UsersGroup>()
                .ToArray();
		}

        /// <summary>
        /// Gets the associated users group for the specified user including inherited ones.
        /// </summary>
        /// <param name="user">The user.</param>
        public virtual UsersGroup[] GetAssociatedUsersGroupIncludingInheritedFor(IUser user)
        {
            return session
                .CreateQuery(@"
                    SELECT ug
                    FROM 
                        UsersGroup ug JOIN ug.UsersIncludingInherited u WITH u.id = :user
                    ORDER BY ug.Name
                ")
                .SetParameter("user", user.SecurityInfo.Identifier)
                .SetCacheable(true)
                .List<UsersGroup>()
                .ToArray();
        }
        
        /// <summary>
		/// Gets the users group by its name
		/// </summary>
		/// <param name="groupName">Name of the group.</param>
		public virtual UsersGroup GetUsersGroupByName(string groupName)
		{
			return session.CreateCriteria<UsersGroup>()
                .Add(Restrictions.Eq("Name", groupName))
                .SetCacheable(true)
                .UniqueResult<UsersGroup>();
		}

		/// <summary>
		/// Gets the entities group by its groupName
		/// </summary>
		/// <param name="groupName">The name of the group.</param>
		public virtual EntitiesGroup GetEntitiesGroupByName(string groupName)
		{
            return session.CreateCriteria<EntitiesGroup>()
                .Add(Restrictions.Eq("Name", groupName))
                .SetCacheable(true)
                .UniqueResult<EntitiesGroup>();
		}


		/// <summary>
		/// Gets the groups the specified entity is associated with
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public virtual EntitiesGroup[] GetAssociatedEntitiesGroupsFor<TEntity>(TEntity entity) where TEntity : class
		{
		    ICollection<EntitiesGroup> entitiesGroups =
		        SecurityCriterions.AllGroups(entity)
		            .GetExecutableCriteria(session)
                    .AddOrder(Order.Asc("Name"))
                    .SetCacheable(true)
                    .List<EntitiesGroup>();

		    return entitiesGroups.ToArray();
		}

		/// <summary>
		/// Associates the entity with a group with the specified name
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="groupName">Name of the group.</param>
		public virtual void AssociateEntityWith<TEntity>(TEntity entity, string groupName) where TEntity : class
		{
			EntitiesGroup entitiesGroup = GetEntitiesGroupByName(groupName);
			Guard.Against<ArgumentException>(entitiesGroup == null, "There is no entities group named: " + groupName);

			AssociateEntityWith(entity, entitiesGroup);
		}

		/// <summary>
		/// Associates the entity with the specified group
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="entitiesGroup">The entities group.</param>
		public void AssociateEntityWith<TEntity>(TEntity entity, EntitiesGroup entitiesGroup) where TEntity : class
		{
			Guid key = Security.ExtractKey(entity);

			EntityReference reference = GetOrCreateEntityReference<TEntity>(key);

            // must flush here since the guid.comb ID is not auto flushed until the transaction completes and we need it in in SQL queries
            session.Flush();

            session
                .CreateSQLQuery("DELETE FROM {{EntityReferencesToEntitiesGroups}} WHERE GroupId = :group AND EntityReferenceId = :reference".PrefixTableName())
                .SetParameter("reference", reference.Id)
                .SetParameter("group", entitiesGroup.Id)
                .ExecuteUpdate();
            session
                .CreateSQLQuery("INSERT INTO {{EntityReferencesToEntitiesGroups}} (GroupId,EntityReferenceId) VALUES(:group,:entity)".PrefixTableName())
                .SetParameter("entity", reference.Id)
                .SetParameter("group", entitiesGroup.Id)
                .ExecuteUpdate();

            ClearEntitiesGroupsCache();
		}


		/// <summary>
		/// Associates the user with a group with the specified name
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="groupName">Name of the group.</param>
		public virtual void AssociateUserWith(IUser user, string groupName)
		{
			UsersGroup group = GetUsersGroupByName(groupName);
			Guard.Against(group == null, "There is no users group named: " + groupName);

			AssociateUserWith(user, group);
		}

		/// <summary>
		/// Associates the user with a group with the specified name
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="group">The group.</param>
		public void AssociateUserWith(IUser user, UsersGroup group)
		{
            UsersGroup[] existingUsersGroups = this.GetAssociatedUsersGroupFor(user);

            if (!existingUsersGroups.Contains(group))
            {
                session
                    .CreateSQLQuery("INSERT INTO {{UsersToUsersGroups}} (UserId, GroupId) VALUES(:user, :group)".PrefixTableName())
                    .SetParameter("user", user.SecurityInfo.Identifier)
                    .SetParameter("group", group.Id)
                    .ExecuteUpdate();
            }

            var newGroups = group.AllChildren.ToList();
            newGroups.Add(group);
            newGroups = newGroups.Except(this.GetAssociatedUsersGroupIncludingInheritedFor(user)).ToList();

            foreach (var ug in newGroups)
            {
                session
                    .CreateSQLQuery("INSERT INTO {{UsersToUsersGroupsInherited}} (UserId, GroupId) VALUES(:user, :group)".PrefixTableName())
                    .SetParameter("user", user.SecurityInfo.Identifier)
                    .SetParameter("group", ug.Id)
                    .ExecuteUpdate();
            }

            ClearUsersGroupsCache();
		}

		/// <summary>
		/// Creates the operation with the given name
		/// </summary>
		/// <param name="operationName">Name of the operation.</param>
		/// <returns></returns>
		public virtual Operation CreateOperation(string operationName)
		{
			Guard.Against<ArgumentException>(string.IsNullOrEmpty(operationName), "operationName must have a value");
			Guard.Against<ArgumentException>(operationName[0] != '/', "Operation names must start with '/'");

			Operation op = new Operation {Name = operationName};

			string parentOperationName = Strings.GetParentOperationName(operationName);
			if (parentOperationName != string.Empty) //we haven't got to the root
			{
				Operation parentOperation = GetOperationByName(parentOperationName);
				if (parentOperation == null)
					parentOperation = CreateOperation(parentOperationName);

				op.Parent = parentOperation;
				parentOperation.Children.Add(op);
			}

			session.Save(op);
			return op;
		}

		/// <summary>
		/// Gets the operation by the specified name
		/// </summary>
		/// <param name="operationName">Name of the operation.</param>
		/// <returns></returns>
		public virtual Operation GetOperationByName(string operationName)
		{
            return session.CreateCriteria<Operation>()
             .Add(Restrictions.Eq("Name", operationName))
             .SetCacheable(true)
             .UniqueResult<Operation>();
		}

		/// <summary>
		/// Removes the user from the specified group
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="usersGroupName">Name of the users group.</param>
		public void DetachUserFromGroup(IUser user, string usersGroupName)
		{
			UsersGroup group = GetUsersGroupByName(usersGroupName);
			Guard.Against(group == null, "There is no users group named: " + usersGroupName);

            session
                .CreateSQLQuery("DELETE FROM {{UsersToUsersGroups}} WHERE UserId=:user AND GroupId = :group".PrefixTableName())
                .SetParameter("user", user.SecurityInfo.Identifier)
                .SetParameter("group", group)
                .ExecuteUpdate();

            var allGroups = group.AllChildren.ToList();
            allGroups.Add(group);

            session
                .CreateSQLQuery("DELETE FROM {{UsersToUsersGroupsInherited}} WHERE UserId=:user AND GroupId IN(:groups)".PrefixTableName())
                .SetParameter("user", user.SecurityInfo.Identifier)
                .SetParameterList("groups", allGroups)
                .ExecuteUpdate();

            ClearUsersGroupsCache();
        }

		/// <summary>
		/// Removes the entities from the specified group
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="entitiesGroupName">Name of the entities group.</param>
		public void DetachEntityFromGroup<TEntity>(TEntity entity, string entitiesGroupName) where TEntity : class
		{
			EntitiesGroup entitiesGroup = GetEntitiesGroupByName(entitiesGroupName);
			Guard.Against<ArgumentException>(entitiesGroup == null,
			                                 "There is no entities group named: " + entitiesGroupName);
			Guid key = Security.ExtractKey(entity);

			EntityReference reference = GetOrCreateEntityReference<TEntity>(key);

            session
                .CreateSQLQuery("DELETE FROM {{EntityReferencesToEntitiesGroups}} WHERE GroupId = :group AND EntityReferenceId = :reference".PrefixTableName())
                .SetParameter("reference", reference.Id)
                .SetParameter("group", entitiesGroup.Id)
                .ExecuteUpdate();

            ClearEntitiesGroupsCache();
		}

		/// <summary>
		/// Removes the user from rhino security.
		/// This does NOT delete the user itself, merely reset all the
		/// information that rhino security knows about it.
		/// It also allows it to be removed by external API without violating
		/// FK constraints
		/// </summary>
		/// <param name="user">The user.</param>
		public void RemoveUser(IUser user)
		{
		    session.CreateQuery("DELETE Permission p WHERE p.User = :user")
		        .SetEntity("user", user)
		        .ExecuteUpdate();
            session.CreateSQLQuery("DELETE FROM {{UsersToUsersGroups}} WHERE UserId = :user".PrefixTableName())
                .SetParameter("user", user.SecurityInfo.Identifier)
                .ExecuteUpdate();
            session.CreateSQLQuery("DELETE FROM {{UsersToUsersGroupsInherited}} WHERE UserId = :user".PrefixTableName())
                .SetParameter("user", user.SecurityInfo.Identifier)
                .ExecuteUpdate();

            ClearUsersGroupsCache();
        }

		/// <summary>
		/// Removes the specified permission.
		/// </summary>
		/// <param name="permission">The permission.</param>
		public void RemovePermission(Permission permission)
		{
			session.Delete(permission);
		}

        /// <summary>
        /// Gets the entity type from a security key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>null if not found</returns>
        public Type GetEntityTypeFromSecurityKey(Guid key)
        {
            EntityReference reference = session.CreateCriteria<EntityReference>()
                .Add(Restrictions.Eq("EntitySecurityKey", key))
                .SetCacheable(true)
                .UniqueResult<EntityReference>();
            if (reference == null)
            {
                return null;
            }

            return Type.GetType(reference.Type.Name);
        }
        
        #endregion

		private static List<UsersGroup> Min(List<UsersGroup> first, List<UsersGroup> second)
		{
			if (first.Count == 0)
				return second;
			if (first.Count <= second.Count)
				return first;
			return second;
		}

        private static List<EntitiesGroup> Min(List<EntitiesGroup> first, List<EntitiesGroup> second)
        {
            if (first.Count == 0)
                return second;
            if (first.Count <= second.Count)
                return first;
            return second;
        }

		private EntityReference GetOrCreateEntityReference<TEntity>(Guid key)
		{
			EntityReference reference = session.CreateCriteria<EntityReference>()
                .Add(Restrictions.Eq("EntitySecurityKey", key))
                .UniqueResult<EntityReference>();

			if (reference == null)
			{
				reference = new EntityReference();
				reference.EntitySecurityKey = key;
				reference.Type = GetOrCreateEntityType<TEntity>();
				session.Save(reference);
			}
			return reference;
		}

		private EntityType GetOrCreateEntityType<TEntity>()
		{
			EntityType entityType = session.CreateCriteria<EntityType>()
                .Add(Restrictions.Eq("Name", typeof (TEntity).FullName))
                .SetCacheable(true)
                .UniqueResult<EntityType>();
			if (entityType == null)
			{
				entityType = new EntityType {Name = typeof (TEntity).FullName};
			    session.Save(entityType);
			}
			return entityType;
		}

        private void ClearUsersGroupsCache()
        {
            var sessionFactory = session.SessionFactory;
            sessionFactory.EvictQueries("rhino-security-usersgroupscollections");

            sessionFactory.EvictCollection("Rhino.Security.Model.UsersGroup.Users");
            sessionFactory.EvictCollection("Rhino.Security.Model.UsersGroup.UsersIncludingInherited");
            sessionFactory.EvictCollection("Rhino.Security.Model.UsersGroup.DirectChildren");
            sessionFactory.EvictCollection("Rhino.Security.Model.UsersGroup.AllChildren");
            sessionFactory.EvictCollection("Rhino.Security.Model.UsersGroup.AllParents");

            //foreach (var collectionMetaData in sessionFactory.GetAllCollectionMetadata().Values)
            //{
            //    var collectionPersister = collectionMetaData as NHibernate.Persister.Collection.ICollectionPersister;
            //    if (collectionPersister != null)
            //    {
            //        if ((collectionPersister.Cache != null) && (collectionPersister.Cache.RegionName == regionName))
            //        {
            //            sessionFactory.EvictCollection(collectionPersister.Role);
            //        }
            //    }
            //}
        }
        private void ClearEntitiesGroupsCache()
        {
            var sessionFactory = session.SessionFactory;
            sessionFactory.EvictQueries("rhino-security-entitiesgroupscollections");

            sessionFactory.EvictCollection("Rhino.Security.Model.EntitiesGroup.Entities");
            sessionFactory.EvictCollection("Rhino.Security.Model.EntitiesGroup.DirectChildren");
            sessionFactory.EvictCollection("Rhino.Security.Model.EntitiesGroup.AllChildren");
            sessionFactory.EvictCollection("Rhino.Security.Model.EntitiesGroup.AllParents");
        }
    }
}
