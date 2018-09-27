#version 420 core
uniform sampler2D tex;
in vec2 fragTexCoord;
layout(location = 0) out vec4 finalColor;
void main()
{
	finalColor = texture(tex, fragTexCoord);
}