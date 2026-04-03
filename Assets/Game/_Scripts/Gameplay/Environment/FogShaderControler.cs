using UnityEngine;

namespace Game.Gameplay.Environment
{
    public class FogShaderControler : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public Material fogMaterial;
        public Transform playerTransform;

        void Update()
        {
            if (fogMaterial != null && playerTransform != null)
            {
                // Truyền tọa độ nhân vật vào Shader
                fogMaterial.SetVector("_PlayerPos", playerTransform.position);
            }
        }
    }
}
