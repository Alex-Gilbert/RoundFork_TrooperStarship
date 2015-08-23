Shader "Custom/SpriteShader" {
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NormalsTex ("Sprite Normals", 2D) = "bump" {}
        _Ramp ("Shading Ramp", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _Cutoff ("Alpha Cutoff", Range (0,1)) = 0.5
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="False" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting On
        ZWrite Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf CustomLambert alpha vertex:vert addshadow alphatest:_Cutoff 
        #pragma multi_compile DUMMY PIXELSNAP_ON

        sampler2D _MainTex;
        sampler2D _NormalsTex;
        sampler2D _Ramp;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color;
        };
        
        half4 LightingCustomLambert (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
            half NdotL = dot (s.Normal, lightDir);
            half4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * (tex2D (_Ramp, half2 (NdotL * 0.5 + 0.5, 0)))) * (atten * 2);
            c.a = s.Alpha;
            return c;
        }
        
        void vert (inout appdata_full v, out Input o)
        {
            #if defined(PIXELSNAP_ON) && !defined(SHADER_API_FLASH)
            v.vertex = UnityPixelSnap (v.vertex);
            #endif
            v.normal = float3(0,0,-1);
            v.tangent = float4(-1, 0, 0, 1);
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = _Color * v.color;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
            o.Albedo = c.rgb;
            o.Normal = UnpackNormal (tex2D (_NormalsTex, IN.uv_MainTex));
            o.Alpha = c.a;
        }
        ENDCG
    } 
    FallBack "Diffuse"
}