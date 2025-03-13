using UnityEngine;

namespace Interfaces.Runtime
{
    public interface IPlayerGraphic
    {
        public void SetMaterialIndex(int index);

        public Material GetMaterial();

        public Material GetEmissiveMaterial();

        public Material GetScoreboardMaterial();

        public void SetLocalPlayerID(int id);

        public int GetLocalPlayerID();

        public int GetMaterialIndex();

        public void UseRandomMeshes();

        public void UseRandomMeshes(object data);

        public object GetRandomCustomisationData();

        public void ForceMaterial(int id, bool useDuoMatarial);

        public void SwitchHighToLowPolyMesh();

        public void SwitchLowToHighPolyMesh();

        public void SetUpInGameAnimator();

        public Animator GetInGameAnimator();

        public Animator GetMenuAnimator();

        public string GetInGameScoreAnimStateName();

        public string GetMenuWinAnimStateName();

        public void ApplyDefaultMaterial();
    }
}