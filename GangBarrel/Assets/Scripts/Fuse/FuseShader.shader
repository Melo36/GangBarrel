Shader "Custom/FuseBurningOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BurnProgress ("Burn Progress", Range(0,1)) = 0
        _UnburntColor ("Unburnt Color", Color) = (0.5,0.25,0.3,1)
        _BurntColor ("Burnt Color", Color) = (0.1,0.1,0.1,1)
        _BurningColor ("Burning Color", Color) = (1,0.5,0,1)
        _BurnWidth ("Burn Width", Range(0,0.2)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BurnProgress;
            float4 _UnburntColor;
            float4 _BurntColor;
            float4 _BurningColor;
            float _BurnWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float burnEdge = _BurnProgress;
                float burningZone = smoothstep(burnEdge - _BurnWidth, burnEdge, i.uv.y) * 
                                  (1 - smoothstep(burnEdge, burnEdge + _BurnWidth, i.uv.y));
                
                float unburnt = step(burnEdge + _BurnWidth, i.uv.y);
                float burnt = step(i.uv.y, burnEdge - _BurnWidth);
                
                fixed4 col = _BurningColor * burningZone +
                            _BurntColor * burnt +
                            _UnburntColor * unburnt;

                // Make sure unburnt parts are fully opaque
                col.a = unburnt + burningZone + burnt * 0.5;
                return col;
            }
            ENDCG
        }
    }
}
