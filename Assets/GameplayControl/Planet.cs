using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.GameplayControl
{
    [Serializable]
    public class Planet
    {
        public string name;
        public int Id { get; set; }
        public Vector3 position;
        public bool withSatellite { set; get; } = false;
        public List<Path> adjacentPaths;
    }
}
