using UnityEngine;

public class SessionSpawnPoints : MonoBehaviour
{
   
    [SerializeField] private Transform[] points = null; //Punto di spawn
    private bool initialized = false;
    private int orderedIndex = -1; //Indice che tiene traccia dell'ordine dei punti di spawn (utilizzato per il metodo di spawn ordinato).

    private static SessionSpawnPoints singleton = null; //Riferimento statico al singleton della classe.

    public static SessionSpawnPoints Singleton //Proprietà statica per ottenere l'istanza unica della classe.
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<SessionSpawnPoints>();
                singleton.Initialize();
            }
            return singleton; 
        }
    }
    
    private void OnDestroy() //Metodo chiamato quando l'oggetto viene distrutto.
    {
        if (singleton == this)
        {
            singleton = null;
        }
    }
    
    private void Initialize() //Metodo di inizializzazione
    {
        if (initialized) { return; }
        initialized = true;
        orderedIndex = -1;
    }
    
    private void OnDrawGizmos() //Metodo per disegnare gizmos nella scena durante l'editor di Unity, utile per il debug.
    {
        if (points != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < points.Length; i++)
            {   //Per ogni punto di spawn, se il `Transform` non è nullo, disegna una sfera nella posizione del punto.
                if (points[i] != null)
                {
                    Gizmos.DrawSphere(points[i].position, 0.1f);
                }
            
            }
        }
    }
    
    public Vector3 GetSpawnPosition(int index) //Metodo che restituisce la posizione di spawn per un dato indice.
    {
        if (index >= 0 && index < points.Length && points[index] != null)
        {
            return points[index].position;
        }
        return Vector3.zero;
    }
    
    public Vector3 GetSpawnPositionRandom() //Metodo che restituisce una posizione di spawn casuale.
    {
        return GetSpawnPosition(UnityEngine.Random.Range(0, points.Length));
    }
    
    public Vector3 GetSpawnPositionOrdered() //Metodo che restituisce la posizione di spawn seguendo un ordine ciclico.
    {
        orderedIndex++;
        if (orderedIndex >= points.Length)
        {
            orderedIndex = 0;
        }
        return GetSpawnPosition(orderedIndex);
    }
    
}