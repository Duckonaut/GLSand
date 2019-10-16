#version 330 core
in float time;
out vec4 FragColor;

void main()
{
	vec2 iResolution = vec2(640, 360);
	
	vec2 uv = vec2(gl_FragCoord.x / iResolution.x, gl_FragCoord.y / iResolution.y);
    
    vec2 center = vec2(0.5, 0.5);

    vec2 rel = uv - center;
    
	rel.x = rel.x * 640.0 / 360.0;

	float t = -time;
    
    float angle = atan(rel.y, rel.x);

    float displacement = 1.0 + sin(angle * 10.0 + t) * sin(angle * 3.0 - t) * 0.05;

    vec4 color = vec4(1,1,1,1);

    float distance = length(rel);
    float maxBound = length(rel * displacement);

    if (distance < 0.5 - (displacement - 1.0) / 2.0)
    {
        distance *= 0.6;
        angle += 3.14;
        angle += distance * 4.0 + sin(distance * 6.28) * 2.0;
        angle -= mod(t, 6.28) / 4.0;
        bool b = mod(angle, (3.14 / 4.0)) < (3.14 / 8.0);

        float m = sin(angle * 8.0);

        color.r = m > 0.0 ? 64.0 / 255.0 * m : 76.0 / 255.0 * -m;
        color.g = m > 0.0 ? 42.0 / 255.0 * m : 208.0 / 255.0 * -m;
        color.b = m > 0.0 ? 149.0 / 255.0 * m : -m;
        distance *= 2.0;
        color.rgb *= clamp((-0.6666667 * distance) + (7.222222 * distance * distance) - (5.555556 * distance * distance * distance), 0.0, 1.0);
    }
    else if (distance > 0.5 - (displacement - 1.0) / 2.0 && distance < 0.5 - (displacement - 1.0) / 2.0)
    {
        color = vec4(1,1,1,1);
    }
    
    

    FragColor = color;
}