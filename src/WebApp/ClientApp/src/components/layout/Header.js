import React from "react";
import { Link } from "@reach/router";

const NavLink = props => (
  <Link
    {...props}
    getProps={({ isCurrent }) => {
      // the object returned here is passed to the
      // anchor element's props
      return {
        style: {
          color: isCurrent ? "#F15B2A" : ""
        }
      };
    }}
  />
);

const Header = () => {
  return (
    <nav>
      <NavLink to="/">Home</NavLink>
      <NavLink to="accounts">Accounts</NavLink>
    </nav>
  );
};

export default Header;
