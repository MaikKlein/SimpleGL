#version 400
 
in vec3 vPosition;
in vec3 vColor;
in vec3 vOffset;
out vec4 color;
uniform mat4 mview;
void main()
{
    gl_Position =  mview * vec4(vPosition + vOffset, 1.0);
    color = vec4( vColor, 1.0);
}
