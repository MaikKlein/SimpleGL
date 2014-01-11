#version 400
 
in vec3 vPosition;
in vec3 vNormal;

out vec4 color;

uniform mat4 normalMatrix;
uniform mat4 mvp;
uniform mat4 modelView;
uniform vec4 lightPos;

void main()
{   
    vec4 tnorm = normalize(normalMatrix * vec4(vNormal, 0.0));
    vec4 eyeCord= modelView * vec4(vPosition, 1.0);
    vec4 s = normalize(lightPos - eyeCord) ;
    float angle = max(dot(tnorm,s),0.0);

    gl_Position = mvp * vec4(vPosition, 1.0);
    color = angle * vec4(1,0,0,1) + vec4(0.1,0.1,0.1,1);
}
