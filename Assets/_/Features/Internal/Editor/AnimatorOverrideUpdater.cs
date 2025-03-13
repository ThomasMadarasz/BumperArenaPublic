using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Internal.Runtime
{
    public class AnimatorOverrideUpdater : MonoBehaviour
    {
        [SerializeField] private RuntimeAnimatorController _runtimeAnimator;
        [SerializeField] private Object[] _characters;

        [SerializeField] private string _overrideAnimatorPath;
        [SerializeField] private string _meshPath;

        [Button()]
        private void ImportAnimationInOverrideController()
        {
            bool isFirst = true;
            int i = 0;

            foreach (var item in _characters)
            {
                AnimatorOverrideController aoc = CreateOverrideController(item.name);
                List<AnimationClip> clips = GetAnimationClipFor(item.name);

                var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                foreach (var a in aoc.animationClips)
                {
                    AnimationClip clip = clips.FirstOrDefault(x => x.name.Split('|')[1] == a.name); ;

                    //if (isFirst)
                    //{
                    //    clip = clips.FirstOrDefault(x => x.name.Split('|')[1] == a.name);
                    //}
                    //else
                    //{
                    //    //string clipName = a.name;
                    //    //string[] fullName = clipName.Split('|');
                    //    //clipName = fullName[0];

                    //    //char playerIndex = clipName[clipName.Length - 1];

                    //    //int indexValue = _index.IndexOf(playerIndex);
                    //    //clipName = clipName.Replace($"_{playerIndex}", $"_{_index[indexValue + i]}");

                    //    //clipName += "|" + fullName[1];

                    //    clip = clips.FirstOrDefault(x => x.name == a.name);
                    //}

                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, clip));
                }

                isFirst = false;
                i++;

                aoc.ApplyOverrides(anims);
            }
        }


        private List<AnimationClip> GetAnimationClipFor(string objectName)
        {
            //Object[] objs = AssetDatabase.LoadAllAssetsAtPath("Assets/_/Content/Art/Meshes/Character/Ingame/mesh_Driver_InGame_A" + ".fbx");
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath($"{_meshPath}/{objectName}" + ".fbx");
            List<AnimationClip> animList = new();

            foreach (var item in objs)
            {
                if (item is AnimationClip)
                {
                    if (item.name.Contains("__preview")) continue;
                    if (item.name.Contains("Lootbox")) continue;
                    if (item.name.Contains("Galaxy_Trap_Bumper")) continue;
                    animList.Add(item as AnimationClip);
                }
            }

            return animList;
        }

        private AnimatorOverrideController CreateOverrideController(string controllerName)
        {
            AnimatorOverrideController aoc = new(_runtimeAnimator);

            string path = Path.Combine(_overrideAnimatorPath, $"{controllerName}.controller");
            AssetDatabase.CreateAsset(aoc, path);
            return AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
        }


        private List<char> _index = new() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V' };

    }
}