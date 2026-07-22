using System;
using System.Collections.Generic;
using UnityEngine;
using UniGLTF;

#if UNITY_EDITOR
using UnityEditor;

namespace UniVRM10
{
    public static class PreviewMaterialUtil
    {
        public static bool TryCreateForPreview(Material material, out PreviewMaterialItem item)
        {
            item = new PreviewMaterialItem(material);

            var propNames = new List<string>();
            var hasError = false;
            for (int i = 0; i < material.shader.GetPropertyCount(); ++i)
            {
                var propType = material.shader.GetPropertyType(i);
                var name = material.shader.GetPropertyName(i);

                switch (propType)
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        // 色
                        {
                            if (!PreviewMaterialItem.TryGetBindType(name, out var bindType))
                            {
                                UniGLTFLogger.Error($"{material.shader.name}.{name} is unsupported property name");
                                hasError = true;
                                continue;
                            }

                            if (!Enum.TryParse(propType.ToString(), true, out ShaderPropertyType propertyType))
                            {
                                UniGLTFLogger.Error($"{material.shader.name}.{propertyType.ToString()} is unsupported property type");
                                hasError = true;
                                continue;
                            }

                            item.Materials[0].PropMap.Add(bindType, new PropItem
                            {
                                Name = name,
                                PropertyType = propertyType,
                                DefaultValues = material.GetColor(name),
                            });
                            propNames.Add(name);
                        }
                        break;

                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        break;
                }
            }
            item.PropNames = propNames.ToArray();

            return !hasError;
        }
    }
}
#endif