shader_type spatial;
render_mode unshaded;

 //uniform sampler2D albedo_texture: source_color, filter_nearest;
uniform sampler2D left_texture: filter_nearest;
uniform sampler2D right_texture: filter_nearest;
void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	//ALBEDO = VIEW_INDEX == VIEW_MONO_LEFT ? vec3(255, 0, 0) : vec3(0, 255, 0);
	//ALBEDO = texture(albedo_texture, UV).rgb;
	//ALBEDO = texture(VIEW_INDEX == VIEW_MONO_LEFT ? left_texture : right_texture, UV).rgb;
	ALBEDO = VIEW_INDEX == VIEW_MONO_LEFT ? (texture(left_texture, UV).rgb) : (texture(right_texture, UV).rgb);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
