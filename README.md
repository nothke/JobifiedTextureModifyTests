# JobifiedTextureModifyTests

Various texture modification tests using jobs and Burst compiler.

Also includes a jobified Burst-ed Mandelbrot vs GPU implementation. You can get a binary of it in Releases.

## Some conclusions:

* Yes, it's possible to assign pixels to the texture via Jobs, and MUCH MUCH faster than classic C# way.
* Use `texture.GetRawTextureData<T>();` to get the NativeArray of the pixels. You are directly working on the native array of pixels and there are no copies on the CPU side needed.
* Use `texture.Apply(false)` to upload the data to GPU, and `false` to not generate mipmaps as it is MUCH (like 10x) faster than the one that generates mipmaps. If you need mipmaps you can create them on your own.
* The type passed to `texture.GetRawTextureData<T>()` needs to match the format of the texture.
* Size of data matters for speed. Use `texture.GetRawTextureData<Color32>` if possible (if not using HDR).
* Manipulating textures on the GPU is still a much preferred option. It not only is usually faster, but also frees the CPU of precious time. If you don't need the interaction with the CPU, or the interaction is minimal, then implement modifications in a shader.
