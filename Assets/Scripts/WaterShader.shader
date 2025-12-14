Shader "Unlit/WaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.0,0.3,0.5,1)

        [Header(Wave Settings)]
        _WaveSpeed("wave Speed", Range(0, 5)) = 1.0
        _WaveHeight("wave Height", Range(0, 2)) = 0.2
        _WaveFrequency("wave Frequency", Range(0, 2)) = 0.5
        _WaveDirection("Wave Direction", Vector) = (1,0,0,0)
        _WaveRotation("Wave Rotation", Range(0,6.28)) = 1.25

        [Header(Wave Dampeners)]
        _FrequencyDampener("Frequency Dampener", Range(0,5)) = 2.0
        _HeightDampener("Height Dampener", Range(0,3)) = 0.5
        _SpeedDampener("Speed Dampener", Range(0,5)) = 1.1

        [Header(Lighting)]
        _Specular("Specular", Range(10,300)) = 100
        _LightDirection("Light Direction", Vector) = (0,1,0,0)
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float _WaveSpeed;
            float _WaveHeight;
            float _WaveFrequency;
            float4 _WaveDirection;
            float _WaveRotation;
            
            float _FrequencyDampener;
            float _HeightDampener;
            float _SpeedDampener;

            float _Specular;
            float4 _LightDirection;


            float2 rotate(float2 v, float angle){
                float s = sin(angle );
                float c = cos(angle);
                float2 r;
                // formula for 2D rotation
                r.x = c * v.x - s* v.y;
                r.y = s * v.x + c * v.y;
                return r;
            }

            v2f vert (appdata v)
            {
                v2f o;
                // Store world position
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 currentDir = normalize(_WaveDirection.xz);
                float dx = 0.0;
                float dz = 0.0;

                for(int i = 0; i < 4; i++){
                    float2 D = currentDir; 
                    float2 direction = dot(D, worldPos.xz);

                    float phase = direction * _WaveFrequency + _Time.y * _WaveSpeed ;
                    worldPos.y += sin(phase) * _WaveHeight;

                    float derivative = _WaveFrequency * _WaveHeight * cos(phase);

                    dx += derivative * D.x;
                    dz += derivative * D.y;
                    

                    _WaveFrequency*= _FrequencyDampener;
                    _WaveHeight *= _HeightDampener;
                    _WaveSpeed *= _SpeedDampener;
                    currentDir = rotate(currentDir, _WaveRotation); 
                }

                o.worldPos = worldPos;
                // Normal = (d/dx , 1, d/dz) = T x B
                o.normal = normalize(float3(-dx, 1.0, -dz));
                
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                
                return o; 
            }



            // Recalculate Normals 
            // calculate the slope based on the derivative of the wave function 
            // δ/δx = (sin(x+1)-sin(x-1))/((x+1)- (x-1)) ---> this method is used with the neighboring vertices to approximate the normal
            // instead we can calculate the partial derivatives using calculus (one for the x direction and one for the z direction)
            // then we reconstruct the Tangent and Binormal  (these are the neighbor vectors on the surface)
            // this is taking into a account z is the up direction
            // T = <1,0, δ/δx>
            // B = <0,1, δ/δz>
            // Normal N = T x B
            // N has to be normalized after the cross product
            // then : N * L = |N|*|L|cos(θ) ------ L being the direction to the light source (also normalized)
            fixed4 frag (v2f i) : SV_Target
            {
                
                // Partial derivatives
                float lightDir = normalize(_LightDirection.xyz);
                
                // saturate to clamp between 0 and 1
                // clamping is important to avoid negative lighting values which can cause artifacts
                float NdotL = max(0, dot(i.normal, lightDir)); // ambient term

                return _Color * (NdotL); // basic diffuse lighting with ambient term
                // return _Color;
            }
            ENDCG
        }
    }
}
