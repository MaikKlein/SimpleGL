module Test

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open SimpleGL


type Game() = 
    inherit GameWindow(640, 360, GraphicsMode.Default, 
                       "F# SimpleGL Sample")

    
    override o.OnLoad e = 
        base.OnLoad(e)
       // let lightPos = 
       //     Vector4.Transform(Vector4(2.0f, 10.0f, -8.0f, 1.0f), view)
       // program.UniForm4 (program.GetUniformLocation "lightPos") (lightPos)
       // program.UniformMatrix4 (program.GetUniformLocation "proj") proj
       // program.UniformMatrix4 (program.GetUniformLocation "view") view
       // let mutable m = Matrix4.CreateRotationX(toRad count)
       // let p = (program.GetUniformLocation "model")
       // GL.UniformMatrix4(p,false,&m)
    override o.OnResize e = 
        base.OnResize e
        GL.Enable(EnableCap.DepthTest)
        GL.Viewport
            (base.ClientRectangle.X, base.ClientRectangle.Y, 
             base.ClientRectangle.Width, base.ClientRectangle.Height)
    
    override o.OnUpdateFrame e = 
        base.OnUpdateFrame e
        if base.Keyboard.[Key.Escape] then base.Close()
    
    override o.OnRenderFrame(e) = 
        base.OnRenderFrame e
        GL.Clear
            (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        
        base.SwapBuffers()
