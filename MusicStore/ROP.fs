module ROP

type Result<'a, 'b> =
| Success of 'a
| Failure of 'b