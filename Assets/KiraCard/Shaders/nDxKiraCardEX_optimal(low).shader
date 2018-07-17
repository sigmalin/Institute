
Shader "nDxShasder/nDxKiraCardEX Optimal(Low Device)" {
    Properties {
        _MainTex ("Art Tex", 2D) = "white" {}
        _ArtMetal ("Art Metal", Range(0, 1)) = 1
        _ArtGloss ("Art Gloss", Range(0, 1)) = 0.2136752
        _ArtEmission ("Art Emission", 2D) = "black" {}
        _FrameTex ("Frame Tex", 2D) = "white" {}
        _FrameNormal ("Frame Normal", 2D) = "bump" {}
        _FrameColor ("Frame Color", Color) = (0.8970588,0.7652294,0.4815095,1)
        _FrameMetal ("Frame Metal", Range(0, 1)) = 1
        _FrameGloss ("Frame Gloss", Range(0, 1)) = 0.7777783
        _KiraTex ("Kira Tex", 2D) = "bump" {}
        [HDR]_KiraColor ("Kira Color", Color) = (1,1,1,1)
        _KiraPower ("Kira Power", Range(0, 1)) = 0
        _KiraTile ("Kira Tile", Range(0, 1)) = 0.5555556
        _KiraAngle ("Kira Angle", Range(0, 2)) = 0
        _KiraMetal ("Kira Metal", Range(0, 1)) = 1
        _KiraGloss ("Kira Gloss", Range(0, 1)) = 0.7264957
        _SubKiraTex ("Sub Kira Tex", 2D) = "bump" {}
        _SubKiraPower ("Sub Kira Power", Range(0, 1)) = 1
        _SubKiraTile ("Sub Kira Tile", Range(0, 1)) = 1
        _SubKiraAngle ("Sub Kira Angle", Range(0, 2)) = 0
        _HoloTex ("Holo Tex", 2D) = "white" {}
        _HoloShift ("Holo Shift", Range(0, 2)) = 2
        _HoloBrightness ("Holo Brightness", Range(0, 1)) = 0.5
        _CardDistortion ("Card Distortion", Range(0, 1)) = 0
        _NisuTex ("Nisu Tex", 2D) = "black" {}
        _NisuNormal ("Nisu Normal", 2D) = "bump" {}
        _NisuNormalPower ("Nisu Normal Power", Range(0, 1)) = 0
        _SkyBox ("SkyBox", Cube) = "_Skybox" {}
        _SkyBoxColor ("SkyBox Color", Color) = (0.5,0.5,0.5,1)
        [MaterialToggle] _Gradation ("Use Gradation", Float ) = 0.5
        _GradColor ("Grad Color", Color) = (0.5,0.5,0.5,1)
        _GradAngle ("Grad Angle", Range(0, 2)) = 0
        _BackFace ("BackFace", 2D) = "white" {}
        [MaterialToggle] _AmbientColor ("Use Ambient Color", Float ) = 1
        [MaterialToggle] _GI ("Use GI", Float ) = 0

        [HideInInspector] _InvKiraTile ("1/Kira Tile", Float) = 0
        [HideInInspector] _InvKiraTileFloor ("Floor(1/Kira Tile)", Float) = 0
        [HideInInspector] _CosKiraAngle ("Cos(Kira Angle)", Float) = 0
        [HideInInspector] _SinKiraAngle ("Sin(Kira Angle)", Float) = 0

        [HideInInspector] _InvSubKiraTile ("1/Sub Kira Tile", Float) = 0
        [HideInInspector] _InvSubKiraTileFloor ("Floor(1/Sub Kira Tile)", Float) = 0
        [HideInInspector] _CosSubKiraAngle ("Cos(Sub Kira Angle)", Float) = 0
        [HideInInspector] _SinSubKiraAngle ("Sin(Sub Kira Angle)", Float) = 0

        [HideInInspector] _CosGradAngle ("Cos(Gradation Angle)", Float) = 0
        [HideInInspector] _SinGradAngle ("Sin(Gradation Angle)", Float) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma shader_feature _ USE_AMBIENT_COLOR
            #pragma shader_feature _ USE_GRADATION
            #pragma shader_feature _ USE_GI      
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal xboxone ps4 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _FrameNormal; uniform float4 _FrameNormal_ST;
            uniform float4 _FrameColor;
            uniform sampler2D _FrameTex; uniform float4 _FrameTex_ST;
            uniform sampler2D _BackFace; uniform float4 _BackFace_ST;
            uniform float _KiraTile;
            uniform sampler2D _SubKiraTex; uniform float4 _SubKiraTex_ST;
            uniform float _KiraAngle;
            uniform float _SubKiraTile;
            uniform float _KiraMetal;
            uniform float _KiraGloss;
            uniform float _KiraPower;
            uniform float _HoloShift;
            uniform float _HoloBrightness;
            uniform float4 _KiraColor;
            uniform float _CardDistortion;
            uniform samplerCUBE _SkyBox;
            uniform float _SubKiraPower;
            uniform sampler2D _HoloTex; uniform float4 _HoloTex_ST;
            uniform sampler2D _KiraTex; uniform float4 _KiraTex_ST;
            uniform float4 _SkyBoxColor;
            uniform float _GradAngle;
            uniform float4 _GradColor;
            uniform fixed _Gradation;
            uniform float _ArtMetal;
            uniform float _ArtGloss;
            uniform sampler2D _ArtEmission; uniform float4 _ArtEmission_ST;
            uniform float _FrameMetal;
            uniform float _FrameGloss;
            uniform sampler2D _NisuTex; uniform float4 _NisuTex_ST;
            uniform sampler2D _NisuNormal; uniform float4 _NisuNormal_ST;
            uniform float _NisuNormalPower;
            uniform float _SubKiraAngle;
            uniform fixed _AmbientColor;
            uniform fixed _InvKiraTile;
            uniform fixed _InvKiraTileFloor;
            uniform fixed _CosKiraAngle;
            uniform fixed _SinKiraAngle;
            uniform fixed _InvSubKiraTile;
            uniform fixed _InvSubKiraTileFloor;
            uniform fixed _CosSubKiraAngle;
            uniform fixed _SinSubKiraAngle;
            uniform fixed _CosGradAngle;
            uniform fixed _SinGradAngle;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 localview : TEXCOORD2;
				float3 locallight : TEXCOORD3;
                float4 dots : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };

            float4x4 InvTangentMatrix(float3 tan, float3 bin, float3 nor) 
            {
                float4x4 mat = float4x4(
                    float4(tan, 0),
                    float4(bin, 0),
                    float4(nor, 0),
                    float4(0, 0, 0, 1)
                );

                return transpose(mat);
            }

            inline float3 SamplerKiraNormal(float2 _uv, sampler2D _normal, float4 _normalST, float _tile, float _invTile, float _invTileFloor, float _cos, float _sin)
            {
                float2 samplerUV = (mul(_uv-float2(0.5,0.5),float2x2( _cos, -_sin, _sin, _cos))+float2(0.5,0.5));
                samplerUV += float2(1.0 - _tile * _invTileFloor, _invTileFloor);
                samplerUV *= _invTile;                
                return UnpackNormal(tex2D(_normal,samplerUV.xy * _normalST.xy + _normalST.zw));
            }

            inline float3 CombineNormal(float3 _base, float3 _detail)
            {
                _base += float3(0,0,1);
                _detail *= float3(-1,-1,1);
                return _base * dot(_base, _detail) / _base.z - _detail;
            }

            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);

                float3 b = cross(v.normal, v.tangent);
                float3 view = ObjSpaceViewDir(v.vertex);
				float3 light = ObjSpaceLightDir(v.vertex);
                float4x4 invTangent = InvTangentMatrix(v.tangent, b, v.normal);
				o.localview = mul(view, invTangent);
				o.locallight = mul(light, invTangent);

                v.normal = mul(v.normal, invTangent);

                float3 halfDirection = normalize(o.localview+o.locallight);
                o.dots.x = saturate(dot(o.locallight, halfDirection));   // LdotH
                o.dots.y = dot(halfDirection, v.normal);                 // HdotN
                o.dots.z = saturate(dot(o.localview, halfDirection));    // VdotH
                o.dots.w = dot(o.locallight,v.normal);                   // LdotN

                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = step(0, facing);//( facing >= 0 ? 1 : 0 );

                float3 viewDirection   = normalize(i.localview);
                float3 PlaneNormal = float3(0,0,1);

                // Calc Normal of face side 
                float3 frameN = UnpackNormal(tex2D(_FrameNormal,TRANSFORM_TEX(i.uv0, _FrameNormal)));
                float3 kiraN = SamplerKiraNormal(i.uv0, _KiraTex, _KiraTex_ST, _KiraTile, _InvKiraTile, _InvKiraTileFloor, _CosKiraAngle, _SinKiraAngle);
                float3 subKiraN = SamplerKiraNormal(i.uv0, _SubKiraTex, _SubKiraTex_ST, _SubKiraTile, _InvSubKiraTile, _InvSubKiraTileFloor, _CosSubKiraAngle, _SinSubKiraAngle);
                float3 combineKiraN = CombineNormal(lerp(PlaneNormal,saturate(kiraN),_KiraPower), lerp(PlaneNormal,subKiraN,_SubKiraPower));
                
                float4 mainD = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 frameD = tex2D(_FrameTex,TRANSFORM_TEX(i.uv0, _FrameTex));

                float3 faceN = lerp(frameN,lerp(combineKiraN,PlaneNormal,mainD.a),frameD.r);                
                float3 nisuN = UnpackNormal(tex2D(_NisuNormal,TRANSFORM_TEX(i.uv0, _NisuNormal)));
                float3 normalDirection = CombineNormal(faceN,lerp(PlaneNormal,nisuN,_NisuNormalPower));
                normalDirection = lerp(PlaneNormal,normalDirection,isFrontFace);

                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection  = normalize(i.locallight);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float4 backD = tex2D(_BackFace,TRANSFORM_TEX(float2((1.0 - i.uv0.x),i.uv0.y), _BackFace));
                float faceG = lerp(_FrameGloss,lerp(_KiraGloss,_ArtGloss,mainD.a),frameD.r);
                float4 nisuD = tex2D(_NisuTex,TRANSFORM_TEX(i.uv0, _NisuTex));
                float gloss = lerp(backD.a,saturate((faceG+(nisuD.r*0.4))),isFrontFace);
                float perceptualRoughness = 1.0 - gloss;
                float roughness = perceptualRoughness * perceptualRoughness;
                //float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
#if defined(USE_GI)
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
#endif

////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = i.dots.x;
                float faceM = lerp(_FrameMetal,lerp(_KiraMetal,_ArtMetal,mainD.a),frameD.r);
                float Metallic = lerp(backD.a,saturate((faceM+nisuD.r)),isFrontFace);
                float3 SkyBox = texCUBE(_SkyBox,viewReflectDirection).rgb;

                float HdotN = i.dots.y;
                float3 holoD = tex2D(_HoloTex,TRANSFORM_TEX((float2(HdotN,HdotN)*_HoloShift), _HoloTex)).rgb;
                holoD = CombineNormal(holoD,combineKiraN);
                holoD = CombineNormal(holoD,lerp(PlaneNormal,kiraN,_CardDistortion));
                holoD = tex2D(_HoloTex,TRANSFORM_TEX(mul( UNITY_MATRIX_V, float4(holoD,0) ).xy, _HoloTex)).rgb;
                
                holoD = lerp(
                    2.0*_HoloBrightness*holoD,
                    1.0-(1.0-2.0*(_HoloBrightness-0.5))*(1.0-holoD),                    
                    step(0.5, _HoloBrightness));

                float3 KiraAlbedo = ((holoD+holoD.b)*_KiraColor.rgb);
#if defined(USE_AMBIENT_COLOR)
                float3 backA = (UNITY_LIGHTMODEL_AMBIENT.rgb*backD.rgb);
                float3 faceA = lerp((_FrameColor.rgb*SkyBox),(UNITY_LIGHTMODEL_AMBIENT.rgb*lerp(KiraAlbedo,mainD.rgb,mainD.a)),frameD.r);
#else
                float3 backA = (backD.rgb);
                float3 faceA = lerp((_FrameColor.rgb*SkyBox),(lerp(KiraAlbedo,mainD.rgb,mainD.a)),frameD.r);
#endif
                float3 diffuseColor = lerp(backA,((SkyBox*nisuD.r)+faceA),isFrontFace); // Need this for specular when using metallic
                float3 specularColor;
                float specularMonochrome;
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, Metallic, /*out*/specularColor, /*out*/specularMonochrome );//DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                //specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = i.dots.z;
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                //specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
////// Emissive:                
                float2 gradUV = (mul(i.uv0-float2(0.5,0.5),float2x2( _CosGradAngle, -_SinGradAngle, _SinGradAngle, _CosGradAngle))+float2(0.5,0.5));
                float LdotN = i.dots.w;
#if defined(USE_GRADATION)
                float3 Gradation = lerp(_SkyBoxColor.rgb,_GradColor.rgb,gradUV.r);
#else                
                float3 Gradation = _SkyBoxColor.rgb;
#endif
                float4 mainE = tex2D(_ArtEmission,TRANSFORM_TEX(i.uv0, _ArtEmission));
#if defined(USE_AMBIENT_COLOR)
                float3 faceE = lerp(float3(0.0,0.0,0.0),lerp((UNITY_LIGHTMODEL_AMBIENT.rgb*lerp(float3(0.0,0.0,0.0),Gradation,smoothstep( 0.0, 1.0, LdotN ))),mainE.rgb,mainD.a),frameD.r);
#else
                float3 faceE = lerp(float3(0.0,0.0,0.0),lerp((lerp(float3(0.0,0.0,0.0),Gradation,smoothstep( 0.0, 1.0, LdotN ))),mainE.rgb,mainD.a),frameD.r);
#endif
                float3 emissive = lerp(float3(0.0,0.0,0.0),faceE,isFrontFace);
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal xboxone ps4 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "DxKiraCardEXShaderGUI"
}
