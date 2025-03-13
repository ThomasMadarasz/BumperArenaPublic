using UnityEngine;

namespace Interfaces.Runtime
{
    public interface ISubjectToAForce
    {
        public void AddExternalForce(Vector3 force);
    }
}