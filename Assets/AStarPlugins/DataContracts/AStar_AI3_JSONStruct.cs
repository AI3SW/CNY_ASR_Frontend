using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICUBE.REST
{
    #region SmileforXmas
    namespace SmileForXmas
    {
        [System.Serializable]
        public class emotionAnalysisGit
        {
            public string img;
            public int re_id;
            public string device_id;
            public string guid;
        }


        [System.Serializable]
        public class emotionAnalysis
        {
            public emotionResults results;
            public float seconds;
            public string trx_id;

        }


        [System.Serializable]
        public class emotionType
        {
            public float angry;
            public float disgust;
            public float fear;
            public float happy;
            public float neutral;
            public float sad;
            public float surprise;
        }


        [System.Serializable]
        public class emotionResultsWithDominant
        {
            public string dominant_emotion;
            public emotionType emotion;

        }
        [System.Serializable]
        public class emotionResults
        {
            public emotionResultsWithDominant results;

        }

        [System.Serializable]
        public class ResearchEntity
        {
            public int avatar_id;
            public string re_desc;
            public int re_id;
            public string re_initials;
            public string re_name;
            public int smile_count;
        }
        [System.Serializable]
        public class ResearchEntities
        {
            public List<ResearchEntity> research_entities;
        }

        [System.Serializable]
        public class entityLightweight
        {
            public int re_id;
            public string re_name;
            public int smile_count;
        }
        [System.Serializable]
        public class rankedEntity
        {
            public List<entityLightweight> results;
        }

        [System.Serializable]
        public class deviceLastSmile
        {
            public string device_id;
            public string last_smile_time;
        }

    }
    #endregion
    #region CNY
    namespace CNY
    {
        [System.Serializable]
        public class CNY_Phrases
        {
            public List<Phrase> data;
        }

        [System.Serializable]
        public class Phrase
        {
            public string word;
            public int word_id;
        }
    }
    #endregion
}