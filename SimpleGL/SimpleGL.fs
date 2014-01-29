module SimpleGL

open System
open System.Drawing
open System.Collections.Generic
open System.Text
open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open System.IO

let toRad x = x * (float32) System.Math.PI / 180.0f

let loadshader (name : String) stype = 
    let address = GL.CreateShader(stype)
    use sr = new StreamReader(name)
    GL.ShaderSource(address, sr.ReadToEnd())
    GL.CompileShader(address)
    let r = GL.GetShaderInfoLog(address)
    Console.WriteLine(r)
    address

type Buffer = 
    abstract Bind : unit -> unit

type IBO(data : int [], usage) = 
    let h = GL.GenBuffer()
    interface Buffer with
        member this.Bind() = 
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, h)
            GL.BufferData
                (BufferTarget.ElementArrayBuffer, 
                 (nativeint) (data.Length * sizeof<int>), data, usage)
            ()

type Instancing(pos : int, components : int, t : VertexAttribPointerType, norm : bool, size : int, pointer : int) = 
    interface Buffer with
        member this.Bind() = 
            GL.EnableVertexAttribArray(pos)
            GL.VertexAttribPointer(pos, components, t, norm, size, pointer)
            GL.VertexAttribDivisor(pos, 1)

type VBO<'T when 'T : (new : unit -> 'T) and 'T : struct and 'T :> ValueType>(target : BufferTarget, size : int, dataSize : int, data : 'T [], pos : int, usage : BufferUsageHint, b : bool) = 
    let h = GL.GenBuffer()
    // new(target : BufferTarget, size : int, data : float [], pos : int, usage : BufferUsageHint, b: bool) =
    //     VBO(target,size,sizeof<float>,data,pos,usage,b)
    member this.Pos = pos
    
    interface Buffer with
        member this.Bind() = 
            GL.EnableVertexAttribArray(this.Pos)
            GL.BindBuffer(target, h)
            GL.BufferData<'T>
                (target, (nativeint) (data.Length * dataSize), data, usage)
            GL.VertexAttribPointer
                (pos, size, VertexAttribPointerType.Float, false, 0, 0)
            if b then GL.VertexAttribDivisor(pos, 1)
            else GL.VertexAttribDivisor(pos, 0)
            ()
    
    member this.Handle = h

type VBO = 
    static member from (target : BufferTarget, size : int, data : float [], 
                        pos : int, usage : BufferUsageHint, b : bool) = 
        VBO(target, size, sizeof<float>, data, pos, usage, b)
    static member from (target : BufferTarget, size : int, data : Vector3 [], 
                        pos : int, usage : BufferUsageHint, b : bool) = 
        VBO(target, size, Vector3.SizeInBytes, data, pos, usage, b)

type Shader(filepath : String, stype : ShaderType) = 
    let h = loadshader filepath stype
    member this.Handle = h

type VertexShader = 
    { shader : Shader }
    static member Create path = 
        { shader = Shader(path, ShaderType.VertexShader) }

type FragmentShader = 
    { shader : Shader }
    static member Create path = 
        { shader = Shader(path, ShaderType.FragmentShader) }

type ShaderProgram(vs : VertexShader, fs : FragmentShader) = 
    
    let h = 
        let program = GL.CreateProgram()
        GL.AttachShader(program, vs.shader.Handle)
        GL.AttachShader(program, fs.shader.Handle)
        GL.LinkProgram(program)
        program
    
    member this.GetAttribLocation(name : String) = 
        GL.GetAttribLocation(this.Handle, name)
    member this.GetUniformLocation(name) = 
        GL.GetUniformLocation(this.Handle, name)
    member this.UniForm4 pos value = GL.Uniform4(pos, ref value)
    member this.Uniform1 h (value : float32) = GL.Uniform1(h, value)
    member this.UniformMatrix4 h (value : Matrix4) = 
        GL.UniformMatrix4(h, false, ref value)
    member this.Handle : int = h
    member this.Use() = GL.UseProgram(this.Handle)

type VAO(buffers) = 
    
    let handle = 
        let h = GL.GenVertexArray()
        GL.BindVertexArray(h)
        buffers |> List.iter (fun (vbo : Buffer) -> vbo.Bind())
        GL.BindVertexArray(0)
        h
    
    member this.Handle = handle
    member this.UnUse() = GL.BindVertexArray(0)
    member this.Use() = GL.BindVertexArray(this.Handle)

let indexDraw (mode, count, t, (vao : VAO)) = 
    vao.Use()
    GL.DrawElements(mode, count, t, 0)

let draw (ptype, first, count, (vao : VAO)) = 
    vao.Use()
    GL.DrawArrays(ptype, first, count)
    vao.UnUse()

type Mesh = 
    { program : ShaderProgram
      ptype : PrimitiveType
      count : int
      vao : VAO }

let render model mesh = 
    mesh.program.UniformMatrix4 (mesh.program.GetUniformLocation "model") model
    draw (mesh.ptype, 0, mesh.count, mesh.vao)

type TranslationMatrix4 = Matrix4

type RotationMatrix4 = Matrix4

type Transform = 
    | Translate of TranslationMatrix4
    | Rotation of RotationMatrix4
    | RelRotation of TranslationMatrix4 * RotationMatrix4
    | Id

type Node = 
    { trans : Transform
      mesh : Mesh Option
      nodeList : Node list }

type BoundingBox = 
    { min : Vector3
      max : Vector3 }

type RenderMesh = 
    { model : Matrix4
      mesh : Mesh }

let renderMesh l = l |> List.iter (fun r -> render r.model r.mesh)

let rec genRenderMeshList m graph renderList = 
    let transform = graph.trans
    let mesh = graph.mesh
    let nodeList = graph.nodeList
    
    let model = 
        match transform with
        | Translate(m1) -> m * m1
        | Rotation(m1) -> m1 * m
        | RelRotation(t, r) -> t * r * m
        | Id -> Matrix4.Identity
    
    let f renderMeshList = 
        async { 
            return match nodeList with
                   | [] -> renderMeshList
                   | _ -> 
                       nodeList |> List.fold (fun acc scene -> 
                                       let l = 
                                           genRenderMeshList model scene 
                                               renderMeshList
                                       l @ acc) []
        }
        |> Async.RunSynchronously
    
    match mesh with
    | Some(m) -> 
        let renderMesh = 
            { model = model
              mesh = m }
        f <| renderMesh :: renderList
    | None -> f renderList

let showFps dt = printfn "%f" (1.0f / dt)