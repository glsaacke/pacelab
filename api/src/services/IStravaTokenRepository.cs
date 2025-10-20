using System;
using System.Collections.Generic;
using api.src.models;

namespace api.src.services
{
    public interface IStravaTokenRepository
    {
        List<StravaToken> GetAllStravaTokens();
        StravaToken? GetStravaTokenById(int id);
        StravaToken? GetStravaTokenByUserId(int userId);
        StravaToken CreateStravaToken(StravaTokenRequest stravaTokenRequest);
        StravaToken? UpdateStravaToken(int id, StravaTokenRequest stravaTokenRequest);
        bool DeleteStravaToken(int id);
    }
}