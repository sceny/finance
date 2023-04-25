"use client";

import {
    Menu,
    MenuItem,
    MenuTrigger,
    MenuPopover,
    MenuList,
} from "@fluentui/react-menu";

import { Link } from "@fluentui/react-link";
import { Button, createDOMRenderer } from "@fluentui/react-components";
import FluentRenderer from "../FluentRenderer";

export default function NavLayout({ children }: { children: React.ReactNode }) {
    const renderer = createDOMRenderer();
    return (
        <FluentRenderer renderer={renderer}>
            <header>
                <Menu>
                    <MenuTrigger>
                        <Button>Menu</Button>
                    </MenuTrigger>
                    <MenuPopover>
                        <MenuList>
                            <MenuItem>
                                <Link href="/">Home</Link>
                            </MenuItem>
                            <MenuItem>
                                <Link href="help/about">About</Link>
                            </MenuItem>
                        </MenuList>
                    </MenuPopover>
                </Menu>
            </header>
            {children}
        </FluentRenderer>
    );
}
