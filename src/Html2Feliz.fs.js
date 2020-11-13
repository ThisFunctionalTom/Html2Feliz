import { Record } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Types.js";
import { option_type, list_type, record_type, string_type } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Reflection.js";
import { FSharpChoice$2 } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Choice.js";
import { join, printf, interpolate, toText, split } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/String.js";
import { map, equalsWith } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Array.js";
import { partialApply, comparePrimitives } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Util.js";
import { collect, map as map_1, singleton, append, delay } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Seq.js";
import { singleton as singleton_1, append as append_1, cons, empty, ofSeq } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/List.js";
import * as htmlparser2 from "htmlparser2";

export class HtmlAttribute extends Record {
    constructor(Name, Value) {
        super();
        this.Name = Name;
        this.Value = Value;
    }
}

export function HtmlAttribute$reflection() {
    return record_type("Html2Feliz.HtmlAttribute", [], HtmlAttribute, () => [["Name", string_type], ["Value", string_type]]);
}

export class HtmlNode extends Record {
    constructor(Name, Attributes, Elements, DirectInnerText) {
        super();
        this.Name = Name;
        this.Attributes = Attributes;
        this.Elements = Elements;
        this.DirectInnerText = DirectInnerText;
    }
}

export function HtmlNode$reflection() {
    return record_type("Html2Feliz.HtmlNode", [], HtmlNode, () => [["Name", string_type], ["Attributes", list_type(HtmlAttribute$reflection())], ["Elements", list_type(HtmlNode$reflection())], ["DirectInnerText", option_type(string_type)]]);
}

export class HtmlDocument extends Record {
    constructor(Elements) {
        super();
        this.Elements = Elements;
    }
}

export function HtmlDocument$reflection() {
    return record_type("Html2Feliz.HtmlDocument", [], HtmlDocument, () => [["Elements", list_type(HtmlNode$reflection())]]);
}

export function $007CText$007CSingleTextChild$007CAttributes$007CChildren$007CComplex$007C(node) {
    const attrs = node.Attributes;
    const hasAttrs = !(attrs.tail == null);
    const children = node.Elements;
    const hasChildren = !(children.tail == null);
    const hasSingleTextChild = ((!hasAttrs) ? (!hasChildren) : false) ? (node.DirectInnerText != null) : false;
    const name = node.Name;
    if (hasSingleTextChild) {
        return new FSharpChoice$2(1, [name, node.DirectInnerText]);
    }
    else {
        const matchValue = [hasChildren, hasAttrs];
        if (matchValue[0]) {
            if (matchValue[1]) {
                return new FSharpChoice$2(4, [name, attrs, children]);
            }
            else {
                return new FSharpChoice$2(3, [name, children]);
            }
        }
        else if (matchValue[1]) {
            return new FSharpChoice$2(2, [name, attrs]);
        }
        else {
            return new FSharpChoice$2(0, node.DirectInnerText);
        }
    }
}

export function formatAttribute(indent, level, attr) {
    const indentStr = Array((indent * level) + 1).join(" ");
    if (attr.Name === "class") {
        const classes = split(attr.Value, [" "], null, 0);
        if ((!equalsWith(comparePrimitives, classes, null)) ? (classes.length === 1) : false) {
            const single = classes[0];
            return toText(interpolate("%P()prop.className \"%P()\"", [indentStr, single]));
        }
        else {
            const multi = classes;
            let classNames;
            let strings;
            let mapping;
            const clo1 = toText(printf("%A"));
            mapping = (clo1);
            strings = map(mapping, multi);
            classNames = join("; ", strings);
            return toText(interpolate("%P()prop.className [ %P() ]", [indentStr, classNames]));
        }
    }
    else {
        return toText(interpolate("%P()prop.%P() \"%P()\"", [indentStr, attr.Name, attr.Value]));
    }
}

export function formatNode(indent, level, node) {
    const line = (level_1, text) => toText(interpolate("%P()%P()", [Array((indent * level_1) + 1).join(" "), text]));
    const nodeBlock = (name, content) => delay(() => append(singleton(line(level, toText(interpolate("Html.%P() [", [name])))), delay(() => append(content, delay(() => singleton(line(level, "]")))))));
    return delay(() => {
        const activePatternResult6082 = $007CText$007CSingleTextChild$007CAttributes$007CChildren$007CComplex$007C(node);
        if (activePatternResult6082.tag === 1) {
            const node_1 = activePatternResult6082.fields[0][0];
            const text_2 = activePatternResult6082.fields[0][1];
            return singleton(line(level, toText(interpolate("Html.%P() \"%P()\"", [node_1, text_2]))));
        }
        else if (activePatternResult6082.tag === 2) {
            const attrs = activePatternResult6082.fields[0][1];
            const name_1 = activePatternResult6082.fields[0][0];
            const arg10 = ofSeq(delay(() => map_1((attr) => formatAttribute(indent, level + 1, attr), attrs)));
            const clo1 = partialApply(1, nodeBlock, [name_1]);
            return clo1(arg10);
        }
        else if (activePatternResult6082.tag === 3) {
            const children = activePatternResult6082.fields[0][1];
            const name_2 = activePatternResult6082.fields[0][0];
            const arg10_1 = ofSeq(delay(() => collect((child) => formatNode(indent, level + 1, child), children)));
            const clo1_1 = partialApply(1, nodeBlock, [name_2]);
            return clo1_1(arg10_1);
        }
        else if (activePatternResult6082.tag === 4) {
            const attrs_1 = activePatternResult6082.fields[0][1];
            const children_1 = activePatternResult6082.fields[0][2];
            const name_3 = activePatternResult6082.fields[0][0];
            const arg10_2 = ofSeq(delay(() => append(map_1((attr_1) => formatAttribute(indent, level + 1, attr_1), attrs_1), delay(() => append(singleton(line(level + 1, "prop.children [")), delay(() => append(collect((child_1) => formatNode(indent, level + 2, child_1), children_1), delay(() => singleton(line(level + 1, "]"))))))))));
            const clo1_2 = partialApply(1, nodeBlock, [name_3]);
            return clo1_2(arg10_2);
        }
        else {
            const text_1 = activePatternResult6082.fields[0];
            return singleton(line(level, toText(interpolate("Html.text \"%P()\"", [text_1]))));
        }
    });
}

export function formatDocument(indent, html) {
    return delay(() => collect((node) => formatNode(indent, 0, node), html.Elements));
}

export function parse(htmlString) {
    const handler = {};
    let nodes = empty();
    let current = empty();
    handler.onopentag = ((name, attributes) => {
        current = cons(new HtmlNode(name, empty(), empty(), void 0), current);
    });
    handler.ontext = ((text) => {
        if (current.tail != null) {
            const parents = current.tail;
            const node = current.head;
            current = cons(new HtmlNode(node.Name, node.Attributes, node.Elements, text), parents);
        }
    });
    handler.onclosetag = ((name_1) => {
        let Elements;
        let pattern_matching_result, node_1, parent, parents_1;
        if (current.tail != null) {
            if (current.tail.tail != null) {
                pattern_matching_result = 0;
                node_1 = current.head;
                parent = current.tail.head;
                parents_1 = current.tail.tail;
            }
            else {
                pattern_matching_result = 1;
            }
        }
        else {
            pattern_matching_result = 1;
        }
        switch (pattern_matching_result) {
            case 0: {
                current = cons((Elements = append_1(parent.Elements, singleton_1(node_1)), new HtmlNode(parent.Name, parent.Attributes, Elements, parent.DirectInnerText)), parents_1);
                break;
            }
            case 1: {
                break;
            }
        }
    });
    const parser = new htmlparser2.Parser(handler);
    parser.write(htmlString);
    return new HtmlDocument(current);
}

