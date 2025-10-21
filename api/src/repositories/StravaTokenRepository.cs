using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.src.models;
using api.src.data;

namespace api.src.repositories
{
    public class StravaTokenRepository : IStravaTokenRepository
    {
        private readonly PaceLabContext _context;
        
        public StravaTokenRepository(PaceLabContext context)
        {
            _context = context;
        }

        public List<StravaToken> GetAllStravaTokens()
        {
            return _context.StravaTokens.ToList();
        }

        public StravaToken? GetStravaTokenById(int id)
        {
            return _context.StravaTokens.FirstOrDefault(st => st.StravaTokenId == id);
        }

        public StravaToken? GetStravaTokenByUserId(int userId)
        {
            return _context.StravaTokens.FirstOrDefault(st => st.UserId == userId);
        }

        public StravaToken CreateStravaToken(StravaTokenRequest stravaTokenRequest)
        {
            var stravaToken = new StravaToken
            {
                UserId = stravaTokenRequest.UserId,
                AccessToken = stravaTokenRequest.AccessToken,
                RefreshToken = stravaTokenRequest.RefreshToken,
                ExpiresAt = stravaTokenRequest.ExpiresAt
            };

            _context.StravaTokens.Add(stravaToken);
            _context.SaveChanges();
            return stravaToken;
        }

        public StravaToken? UpdateStravaToken(int id, StravaTokenRequest stravaTokenRequest)
        {
            var existingStravaToken = _context.StravaTokens.FirstOrDefault(st => st.StravaTokenId == id);
            if (existingStravaToken == null)
                return null;

            existingStravaToken.UserId = stravaTokenRequest.UserId;
            existingStravaToken.AccessToken = stravaTokenRequest.AccessToken;
            existingStravaToken.RefreshToken = stravaTokenRequest.RefreshToken;
            existingStravaToken.ExpiresAt = stravaTokenRequest.ExpiresAt;

            _context.SaveChanges();
            return existingStravaToken;
        }

        public bool DeleteStravaToken(int id)
        {
            var stravaToken = _context.StravaTokens.FirstOrDefault(st => st.StravaTokenId == id);
            if (stravaToken == null)
                return false;

            _context.StravaTokens.Remove(stravaToken);
            _context.SaveChanges();
            return true;
        }
    }
}