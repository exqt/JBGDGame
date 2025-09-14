using System;
using UnityEngine;

class RandomManager : MonoBehaviour {
    static RandomManager _instance;
    public int seed = 42;
    System.Random random;

    public static RandomManager Instance {
        get {
            if (_instance == null) {
                _instance = FindAnyObjectByType<RandomManager>();
                if (_instance == null) {
                    GameObject obj = new GameObject();
                    obj.name = typeof(RandomManager).Name;
                    _instance = obj.AddComponent<RandomManager>();
                    _instance.SetSeed(0);
                }
            }
            return _instance;
        }
    }

    private void Awake() {
        _instance = this;
    }

    public void SetSeed(int seed) {
        this.seed = seed;
        random = new System.Random(seed);
    }

    public int Next(int min, int max) {
        return random.Next(min, max);
    }
}
