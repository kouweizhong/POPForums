﻿using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.ExternalLogin
{
	public class UserAssociationManager : IUserAssociationManager
	{
		public UserAssociationManager(IExternalUserAssociationRepository externalUserAssociationRepository, IUserRepository userRepository)
		{
			_externalUserAssociationRepository = externalUserAssociationRepository;
			_userRepository = userRepository;
		}

		private readonly IExternalUserAssociationRepository _externalUserAssociationRepository;
		private readonly IUserRepository _userRepository;

		public ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalAuthenticationResult externalAuthenticationResult)
		{
			if (externalAuthenticationResult == null)
				throw new ArgumentNullException("externalAuthenticationResult");
			var match = _externalUserAssociationRepository.Get(externalAuthenticationResult.Issuer, externalAuthenticationResult.ProviderKey);
			if (match == null)
				return new ExternalUserAssociationMatchResult {Successful = false};
			var user = _userRepository.GetUser(match.UserID);
			if (user == null)
				return new ExternalUserAssociationMatchResult {Successful = false};
			var result = new ExternalUserAssociationMatchResult
			             {
				             Successful = true,
				             ExternalUserAssociation = match,
				             User = user
			             };
			return result;
		}

		public void Associate(User user, ExternalAuthenticationResult externalAuthenticationResult)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (externalAuthenticationResult != null)
			{
				if (String.IsNullOrEmpty(externalAuthenticationResult.Issuer))
					throw new NullReferenceException("The identity claims contain no issuer.");
				if (String.IsNullOrEmpty(externalAuthenticationResult.ProviderKey))
					throw new NullReferenceException("The identity claims contain no provider key");
				_externalUserAssociationRepository.Save(user.UserID, externalAuthenticationResult.Issuer, externalAuthenticationResult.ProviderKey, externalAuthenticationResult.Name);
			}
		}

		public List<ExternalUserAssociation> GetExternalUserAssociations(User user)
		{
			return _externalUserAssociationRepository.GetByUser(user.UserID);
		}

		public void RemoveAssociation(User user, int externalUserAssociationID)
		{
			var association = _externalUserAssociationRepository.Get(externalUserAssociationID);
			if (association == null)
				return;
			if (association.UserID != user.UserID)
				throw new Exception(String.Format("Can't delete external user association {0} because it doesn't match UserID {1}.", externalUserAssociationID, user.UserID));
			_externalUserAssociationRepository.Delete(externalUserAssociationID);
		}
	}
}