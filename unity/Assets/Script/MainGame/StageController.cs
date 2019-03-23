using System.Linq;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField] private Transform[] startObj = new Transform[4];

    public Vector3[] StartPos { get { return this.startObj.Select(l => l.position).ToArray(); } }
}
