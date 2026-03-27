shader_type canvas_item;

void fragment() {
    vec2 uv = vec2(UV.y, 1.0 - UV.x);

    float y = texture(camera_y, uv).r;

    vec2 chroma = texture(camera_CbCr, uv).rg;

    // NV21 (most likely on Android)
    float cb = chroma.y - 0.5;
    float cr = chroma.x - 0.5;

    float r = y + 1.402 * cr;
    float g = y - 0.344136 * cb - 0.714136 * cr;
    float b = y + 1.772 * cb;

    COLOR = vec4(vec3(r, g, b), 1.0);
}
