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

let loadshader (name : String) stype = 
    let address = GL.CreateShader(stype)
    use sr = new StreamReader(name)
    GL.ShaderSource(address, sr.ReadToEnd())
    GL.CompileShader(address)
    address
type Buffer =
    abstract member Bind:  unit -> unit
type IBO(data: int [],usage) = 
    let h = GL.GenBuffer()
    interface Buffer with
        member this.Bind() = 
            GL.BindBuffer(BufferTarget.ElementArrayBuffer,h)
            GL.BufferData(BufferTarget.ElementArrayBuffer,(nativeint)(data.Length * sizeof<int> ),data,usage)
            ()
type Instancing(pos: int, components:int,t: VertexAttribPointerType,norm:bool,size :int,pointer: int) =
    interface Buffer with
        member this.Bind() =
            GL.EnableVertexAttribArray(pos)
            GL.VertexAttribPointer(pos,components,t,norm,size,pointer)
            GL.VertexAttribDivisor(pos,1)
type VBO(target : BufferTarget, size : int, data : Vector3 [], pos : int, usage : BufferUsageHint, b: bool) = 
    let h = GL.GenBuffer()
    member this.Pos = pos
    
    interface Buffer with
        member this.Bind() = 
            GL.EnableVertexAttribArray(this.Pos)
            GL.BindBuffer(target, h)
            GL.BufferData
                (target, (nativeint) (data.Length * Vector3.SizeInBytes), data, 
                 usage)
            GL.VertexAttribPointer
                (pos, size, VertexAttribPointerType.Float, false, 0, 0)
            if b then GL.VertexAttribDivisor(pos,1) else GL.VertexAttribDivisor(pos,0)
            ()
    
    member this.Handle = h

type Shader(filepath : String, stype : ShaderType) = 
    member this.Handle = loadshader filepath stype

type ShaderProgram(shaders) = 
    member this.GetAttribLocation(name : String) = 
        GL.GetAttribLocation(this.Handle, name)
    member this.GetUniformLocation(name) =
        GL.GetUniformLocation(this.Handle,name)
    member this.Uniform1 h (value : float32) =
        GL.Uniform1(h,value)
    member this.UniformMatrix4 h (value: Matrix4) =
        GL.UniformMatrix4(h,false,ref value)
    member this.Handle : int = 
        let program = GL.CreateProgram()
        shaders |> List.iter (fun (s : Shader) -> 
                       GL.AttachShader(program, s.Handle)
                       ())
        GL.LinkProgram(program)
        program
    
    member this.Use() = GL.UseProgram(this.Handle)

type VAO(buffers) = 
    let handle = 
        let h = GL.GenVertexArray()
        GL.BindVertexArray(h)
        buffers |> List.iter (fun (vbo : Buffer) -> 
                       vbo.Bind())
        GL.BindVertexArray(0)
        h
    
    member this.Handle = handle
    member this.UnUse() = GL.BindVertexArray(0)
    member this.Use() = GL.BindVertexArray(this.Handle)
let indexDraw( mode, count, t, (vao: VAO))=
    vao.Use()
    GL.DrawElements(mode,count,t,0)
let draw( ptype, first, count, (vao : VAO)) = 
    vao.Use()
    GL.DrawArrays(ptype, first, count)
    vao.UnUse()