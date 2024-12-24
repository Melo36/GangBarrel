using System.Collections.Generic;
using UnityEngine;

public class FuseManager : MonoBehaviour
{
    public static FuseManager Instance { get; private set; }
    
    [SerializeField] private GameObject fusePrefab;
    [SerializeField] private ParticleSystem fuseParticlePrefab;
    
    private List<Fuse> activeFuses = new List<Fuse>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Fuse CreateFuse(Vector3[] path, Transform target)
    {
        GameObject fuseObj = Instantiate(fusePrefab);
        Fuse fuse = fuseObj.GetComponent<Fuse>();
        
        if (fuse != null)
        {
            fuse.Initialize(path, target);
            activeFuses.Add(fuse);
            
            // Subscribe to fuse completion
            fuse.OnFuseComplete += () => RemoveFuse(fuse);
        }
        
        return fuse;
    }

    private void RemoveFuse(Fuse fuse)
    {
        activeFuses.Remove(fuse);
    }
}