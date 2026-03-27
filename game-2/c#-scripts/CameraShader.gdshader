shader_type canvas_item;

uniform sampler2D camera_y;
uniform sampler2D camera_CbCr;

void fragment() {
    vec2 uv = vec2(UV.y, 1.0 - UV.x);

    float y = texture(camera_y, uv).r;

    vec2 chroma = texture(camera_CbCr, uv).rg;

    // Explicit decode (no matrix tricks)
    float cb = chroma.x - 0.5;
    float cr = chroma.y - 0.5;

    float r = y + 1.402 * cr;
    float g = y - 0.344 * cb - 0.714 * cr;
    float b = y + 1.772 * cb;

    COLOR = vec4(clamp(vec3(r, g, b), 0.0, 1.0), 1.0);
}