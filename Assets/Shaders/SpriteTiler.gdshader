shader_type canvas_item;

uniform vec2 tileScale = vec2(1, 1);
uniform vec2 tileOffset = vec2(0, 0);

// Takes vertices' local space info
// Translates it into local space infor
// for pixels in fragment shader
varying vec4 local_space;

// Just so I can grab the modulate color
// (vertex color) from the vertex shader
varying vec4 modulate;

// Uses the default vertex shader, except
// it also caches local space data for later
void vertex() 
{
	local_space = MODEL_MATRIX * vec4(VERTEX, 0, 0);	
	modulate = COLOR;
}

// Using local space data for the pixel,
// as well as original resolution data,
// find the UV coordinate of the pixel within
// its local "tile" of the texture
void fragment() 
{
	float tileResX = 1.0 * tileScale.x / TEXTURE_PIXEL_SIZE.x;
	float tileResY = 1.0 * tileScale.y / TEXTURE_PIXEL_SIZE.y ;
	
	vec2 tileRes = vec2(tileResX, tileResY);
	vec2 tileUV = mod(local_space.xy + tileOffset, tileRes) / tileRes;
	COLOR = modulate * texture(TEXTURE, tileUV);
}
