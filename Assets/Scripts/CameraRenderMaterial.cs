﻿using UnityEngine;

namespace Viva
{


    public class CameraRenderMaterial : MonoBehaviour
    {

        [SerializeField]
        private Material filmSepia;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            if (filmSepia != null)
            {
                Graphics.Blit(source, destination, filmSepia);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
        public void setEffectMaterial(Material effectMat)
        {
            filmSepia = effectMat;
        }
        public Material getEffectMat()
        {
            return filmSepia;
        }
    }

}