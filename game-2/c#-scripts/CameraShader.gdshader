shader_type canvas_item;

uniform sampler2D camera_tex;

void fragment() {
    vec2 uv = vec2(UV.y, 1.0 - UV.x);

    float y = texture(camera_tex, uv).r;

    // Expand luminance to RGB
    COLOR = vec4(vec3(y), 1.0);
}