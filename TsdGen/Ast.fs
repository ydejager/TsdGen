namespace TsdGen

module Syntax =
    type NsName = | NsName of string
    type IfaceName = | IfaceName of string
    type PropName = | PropName of string

    type Type =
        | Null
        | String
        | Number
        | Bool
        | List of Type
        | Object of (NsName * IfaceName)
        | Union of Type list

    type Member =
        | Property of PropName * Type:Type

    type Extends = | Extends of NsName * IfaceName

    type Declaration = 
        | Namespace of NsName * Declarations:Declaration list
        | Interface of IfaceName * Extends option * Members: Member list
