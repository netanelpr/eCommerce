import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink,Input,Button } from 'reactstrap';
import {Link, Redirect} from 'react-router-dom';
import './NavMenu.css';
import {Dropdown,DropdownButton} from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import {StoreApi} from "../Api/StoreApi";
import {SearchComponent} from "./SearchComponent";
import {AuthApi} from "../Api/AuthApi";


export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor (props) {
     super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {  
      collapsed: true,
      storeList:[],
      isLoggedIn:this.props.state.isLoggedIn,
      itemToSearch:'',
      logOutActivated:false
    };
    this.storeApi = new StoreApi();
    this.authApi = new AuthApi();

    this.handleInputChange = this.handleInputChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleLogout = this.handleLogout.bind(this);
  }


  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }


  
  async handleLogout(event){
    const logoutStatus = await this.authApi.Logout()
    if(logoutStatus.isSuccess){
      return <Redirect exact to="/"/>

    }
    
  }

    handleInputChange(event) {
      const target = event.target;
      this.setState({
        [target.name]: target.value
      });
    }
    
  handleSubmit(){
    
    const searchItem = async () =>
    {
      const searchForItems = await this.storeApi.searchItems(this.state.itemToSearch);
      console.log(searchForItems);
      return searchForItems
    }
    
  }

  render () {
    const {isLoggedIn, storeList} = this.props.state
      return (
          <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow" expand="md"
                    light>
              <div className="containerNavBar">
                <div className="navBarDiv">
                  <NavbarBrand tag={Link} to="/">Home</NavbarBrand>
                  <label
                      className="labelMargin">{`${this.props.state.userName ? "hello " + this.props.state.userName : ""}`}</label>
                </div>

                <div className="navBarSearch">
                  <SearchComponent/>
                </div>

                <div className="navBarDiv">
                  <NavbarToggler onClick={this.toggleNavbar} className="mr-2"/>
                  <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
                    <ul className="navbar-nav flex-grow">
                      {isLoggedIn ?
                          <NavItem>
                            <NavLink tag={Link} className="text-dark" exact to="/logout">Logout</NavLink>
                          </NavItem>
                          :
                          <NavItem>
                            <NavLink tag={Link} className="text-dark" exact to="/login">Login</NavLink>
                          </NavItem>}
                    </ul>
                  </Collapse>
                  <NavLink tag={Link} className="text-dark" exact to="/Cart">
                    <img src="/Images/cart.png" alt="Cart" class="image"/>
                  </NavLink>
                </div>
              </div>
            </Navbar>

          </header>
      );
    }
}
