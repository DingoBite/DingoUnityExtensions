Shader "UI/DashedLinePixel"
{
    Properties
    {
        _Color      ("Tint"        , Color) = (1,1,1,1)
        _DashSize   ("Dash Length" , Float) = 8          // px
        _GapSize    ("Gap Length"  , Float) = 4          // px
        _Thickness  ("Thickness"   , Float) = 2          // px (rect mode)
        [Toggle] _Orientation ("Vertical" , Float) = 0   // 0-horiz, 1-vert
        [Toggle] _Round       ("Round Dots", Float) = 0  // 0-rect, 1-circle
        _Tiling     ("Tiling factor", Float) = 1         // 1 = default
        _Offset     ("Offset (px)", Float) = 0           // shift pattern
        _RectSize   ("RectSize (auto add RectTransformSizeShaderProvider)", Vector) = (100,100,0,0)   // auto-filled
        _Pivot    ("Pivot (auto add RectTransformSizeShaderProvider)", Vector) = (100,100,0,0)   // auto-filled
        [HideInInspector] _MainTex ("Sprite",2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            fixed4 _Color;
            float  _DashSize, _GapSize, _Thickness, _Orientation, _Round, _Tiling, _Offset;
            float4 _RectSize, _Pivot;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 px = i.uv * _RectSize.xy;

                float2 pivotPx = _Pivot.xy * _RectSize.xy;

                const float axis    = step(0.5,_Orientation);

                float centerPy = px.x;
                float centerPx = px.y;
                
                if (_Orientation > 0.5)
                    centerPx = centerPx - pivotPx.y + _DashSize * 0.5;
                else
                    centerPy = centerPy - pivotPx.x + _DashSize * 0.5;
                
                const float coordPx = lerp(centerPy,centerPx, axis)*_Tiling + _Offset;
                float perp    = lerp(abs(px.y - pivotPx.y),
                                     abs(px.x - pivotPx.x), axis);     // <-- pivot-based
                const float cycle  = _DashSize + _GapSize;
                const float local  = frac(coordPx / cycle) * cycle;

                float alpha;
                if (_Round > 0.5)
                {
                    const float2 d = float2(local - _DashSize*0.5, perp);
                    alpha = step(length(d), _DashSize*0.5);
                }
                else
                {
                    alpha  = step(local, _DashSize);
                    alpha *= step(perp, _Thickness*0.5);
                }

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}
