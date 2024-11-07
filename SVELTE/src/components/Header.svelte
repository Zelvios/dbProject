<script>
    import {DarkMode, Navbar, NavBrand, NavHamburger, NavLi, NavUl} from "flowbite-svelte";
</script>

<Navbar>
    <NavBrand href="/">
        <span class="self-center whitespace-nowrap text-xl font-semibold dark:text-white">dbProject</span>
    </NavBrand>
    <NavHamburger  />
    <NavUl >
        <NavLi href="/">Home</NavLi>
        <NavLi href="/about">About</NavLi>
        <NavLi href="/contact">Contact</NavLi>
    </NavUl>
    <DarkMode />
</Navbar>