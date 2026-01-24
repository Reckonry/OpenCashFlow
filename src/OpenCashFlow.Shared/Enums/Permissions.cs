using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

namespace Shared.Enums
{
    public static class Permissions
    {
        public static class Company
        {
            /// <summary>Company section</summary>
            public static readonly string COMP = "COMP";
            /// <summary>View company details</summary>
            public static readonly string COMP_VIEW = "COMP_VIEW";
            /// <summary>Edit company details</summary>
            public static readonly string COMP_EDIT = "COMP_EDIT";
        }

        public static class Customers
        {
            /// <summary> Customer section </summary>
            public static readonly string CUST = "CUST";
            /// <summary> Create a new customer </summary>
            public static readonly string CUST_NEW = "CUST_NEW";
            /// <summary> View the customer details </summary>
            public static readonly string CUST_VIEW = "CUST_VIEW";
            /// <summary> Edit the customer details </summary>
            public static readonly string CUST_EDIT = "CUST_EDIT";
            /// <summary> Delete the customer </summary>
            public static readonly string CUST_DELE = "CUST_DELE";

            public static class Addresses
            {
                /// <summary> Section for managing customer addresses. </summary>
                public static readonly string CUST_ADDR = "CUST_ADDR";
                /// <summary> Create a new address for a customer. </summary>
                public static readonly string CUST_ADDR_NEW = "CUST_ADDR_NEW";
                /// <summary> View the list of addresses associated with a customer. </summary>
                public static readonly string CUST_ADDR_VIEW = "CUST_ADDR_VIEW";
                /// <summary> Edit an existing customer address. </summary>
                public static readonly string CUST_ADDR_EDIT = "CUST_ADDR_EDIT";
                /// <summary> Delete a customer's address. </summary>
                public static readonly string CUST_ADDR_DELE = "CUST_ADDR_DELE";
            }

            public static class Documents
            {
                /// <summary> Section for managing customer documents. </summary>
                public static readonly string CUST_DOCU = "CUST_DOCU";
                /// <summary> Upload a new document for a customer. </summary>
                public static readonly string CUST_DOCU_NEW = "CUST_DOCU_NEW";
                /// <summary> View customer documents. </summary>
                public static readonly string CUST_DOCU_VIEW = "CUST_DOCU_VIEW";
                /// <summary> Edit metadata or content of an existing document. </summary>
                public static readonly string CUST_DOCU_EDIT = "CUST_DOCU_EDIT";
                /// <summary> Delete a document associated with a customer. </summary>
                public static readonly string CUST_DOCU_DELE = "CUST_DOCU_DELE";
            }

            public static class Emails
            {
                /// <summary> Section for managing customer emails. </summary>
                public static readonly string CUST_MAIL = "CUST_MAIL";
                /// <summary> Create a new email entry for a customer. </summary>
                public static readonly string CUST_MAIL_NEW = "CUST_MAIL_NEW";
                /// <summary> View email details associated with a customer. </summary>
                public static readonly string CUST_MAIL_VIEW = "CUST_MAIL_VIEW";
                /// <summary> Edit an existing email entry for a customer. </summary>
                public static readonly string CUST_MAIL_EDIT = "CUST_MAIL_EDIT";
                /// <summary> Delete an email entry for a customer. </summary>
                public static readonly string CUST_MAIL_DELE = "CUST_MAIL_DELE";
            }

            public static class Feedbacks
            {
                /// <summary> Section for managing customer feedback. </summary>
                public static readonly string CUST_FEBK = "CUST_FEBK";
                /// <summary> Submit a new feedback entry from a customer. </summary>
                public static readonly string CUST_FEBK_NEW = "CUST_FEBK_NEW";
                /// <summary> View customer feedback entries. </summary>
                public static readonly string CUST_FEBK_VIEW = "CUST_FEBK_VIEW";
                /// <summary> Edit an existing feedback entry. </summary>
                public static readonly string CUST_FEBK_EDIT = "CUST_FEBK_EDIT";
                /// <summary> Delete a feedback entry. </summary>
                public static readonly string CUST_FEBK_DELE = "CUST_FEBK_DELE";
            }

            public static class Invoices
            {
                /// <summary> Section for managing customer invoices. </summary>
                public static readonly string CUST_INVO = "CUST_INVO";
                /// <summary> Create a new invoice for a customer. </summary>
                public static readonly string CUST_INVO_NEW = "CUST_INVO_NEW";
                /// <summary> View invoice details for a customer. </summary>
                public static readonly string CUST_INVO_VIEW = "CUST_INVO_VIEW";
                /// <summary> Edit an existing invoice. </summary>
                public static readonly string CUST_INVO_EDIT = "CUST_INVO_EDIT";
                /// <summary> Delete an invoice. </summary>
                public static readonly string CUST_INVO_DELE = "CUST_INVO_DELE";
            }

            public static class Notifications
            {
                /// <summary> Section for managing customer notifications. </summary>
                public static readonly string CUST_NOTI = "CUST_NOTI";
                /// <summary> Create a new notification for a customer. </summary>
                public static readonly string CUST_NOTI_NEW = "CUST_NOTI_NEW";
                /// <summary> View notification details sent to a customer. </summary>
                public static readonly string CUST_NOTI_VIEW = "CUST_NOTI_VIEW";
                /// <summary> Edit an existing customer notification. </summary>
                public static readonly string CUST_NOTI_EDIT = "CUST_NOTI_EDIT";
                /// <summary> Delete a notification. </summary>
                public static readonly string CUST_NOTI_DELE = "CUST_NOTI_DELE";
            }

            public static class Orders
            {
                /// <summary> Section for managing customer orders. </summary>
                public static readonly string CUST_ORDE = "CUST_ORDE";
                /// <summary> Create a new order for a customer. </summary>
                public static readonly string CUST_ORDE_NEW = "CUST_ORDE_NEW";
                /// <summary> View order details for a customer. </summary>
                public static readonly string CUST_ORDE_VIEW = "CUST_ORDE_VIEW";
                /// <summary> Edit an existing customer order. </summary>
                public static readonly string CUST_ORDE_EDIT = "CUST_ORDE_EDIT";
                /// <summary> Delete a customer order. </summary>
                public static readonly string CUST_ORDE_DELE = "CUST_ORDE_DELE";
            }

            public static class Payments
            {
                /// <summary> Section for managing customer payments. </summary>
                public static readonly string CUST_PAYM = "CUST_PAYM";
                /// <summary> Register a new payment for a customer. </summary>
                public static readonly string CUST_PAYM_NEW = "CUST_PAYM_NEW";
                /// <summary> View payment details for a customer. </summary>
                public static readonly string CUST_PAYM_VIEW = "CUST_PAYM_VIEW";
                /// <summary> Edit an existing customer payment record. </summary>
                public static readonly string CUST_PAYM_EDIT = "CUST_PAYM_EDIT";
                /// <summary> Delete a payment record. </summary>
                public static readonly string CUST_PAYM_DELE = "CUST_PAYM_DELE";
            }

            public static class Phones
            {
                /// <summary> Section for managing customer phone numbers. </summary>
                public static readonly string CUST_PHON = "CUST_PHON";
                /// <summary> Add a new phone number for a customer. </summary>
                public static readonly string CUST_PHON_NEW = "CUST_PHON_NEW";
                /// <summary> View customer phone number details. </summary>
                public static readonly string CUST_PHON_VIEW = "CUST_PHON_VIEW";
                /// <summary> Edit an existing customer phone number. </summary>
                public static readonly string CUST_PHON_EDIT = "CUST_PHON_EDIT";
                /// <summary> Delete a customer phone number. </summary>
                public static readonly string CUST_PHON_DELE = "CUST_PHON_DELE";
            }

            public static class Subscriptions
            {
                /// <summary> Section for managing customer subscriptions. </summary>
                public static readonly string CUST_SUBS = "CUST_SUBS";
                /// <summary> Create a new subscription for a customer. </summary>
                public static readonly string CUST_SUBS_NEW = "CUST_SUBS_NEW";
                /// <summary> View customer subscription details. </summary>
                public static readonly string CUST_SUBS_VIEW = "CUST_SUBS_VIEW";
                /// <summary> Edit an existing subscription. </summary>
                public static readonly string CUST_SUBS_EDIT = "CUST_SUBS_EDIT";
                /// <summary> Cancel a customer subscription. </summary>
                public static readonly string CUST_SUBS_DELE = "CUST_SUBS_DELE";
            }

        }

        public static class Projects
        {
            /// <summary> Section for managing projects. </summary>
            public static readonly string PROJ = "PROJ";
            /// <summary> Create a new project. </summary>
            public static readonly string PROJ_NEW = "PROJ_NEW";
            /// <summary> View project details. </summary>
            public static readonly string PROJ_VIEW = "PROJ_VIEW";
            /// <summary> Edit an existing project. </summary>
            public static readonly string PROJ_EDIT = "PROJ_EDIT";
            /// <summary> Delete a project. </summary>
            public static readonly string PROJ_DELE = "PROJ_DELE";

            public static class Milestones
            {
                /// <summary> Section for managing project milestones. </summary>
                public static readonly string PROJ_MILE = "PROJ_MILE";
                /// <summary> Create a new milestone within a project. </summary>
                public static readonly string PROJ_MILE_NEW = "PROJ_MILE_NEW";
                /// <summary> View project milestone details. </summary>
                public static readonly string PROJ_MILE_VIEW = "PROJ_MILE_VIEW";
                /// <summary> Edit an existing project milestone. </summary>
                public static readonly string PROJ_MILE_EDIT = "PROJ_MILE_EDIT";
                /// <summary> Delete a project milestone. </summary>
                public static readonly string PROJ_MILE_DELE = "PROJ_MILE_DELE";
            }

            public static class Resources
            {
                /// <summary> Section for managing project resources. </summary>
                public static readonly string PROJ_RESX = "PROJ_RESX";
                /// <summary> Add a new resource to a project. </summary>
                public static readonly string PROJ_RESX_NEW = "PROJ_RESX_NEW";
                /// <summary> View resources assigned to a project. </summary>
                public static readonly string PROJ_RESX_VIEW = "PROJ_RESX_VIEW";
                /// <summary> Edit an existing resource in a project. </summary>
                public static readonly string PROJ_RESX_EDIT = "PROJ_RESX_EDIT";
                /// <summary> Remove a resource from a project. </summary>
                public static readonly string PROJ_RESX_DELE = "PROJ_RESX_DELE";
            }

            public static class ProjectTasks
            {
                /// <summary> Section for managing project tasks. </summary>
                public static readonly string PROJ_TASK = "PROJ_TASK";
                /// <summary> Create a new task within a project. </summary>
                public static readonly string PROJ_TASK_NEW = "PROJ_TASK_NEW";
                /// <summary> View details of a project task. </summary>
                public static readonly string PROJ_TASK_VIEW = "PROJ_TASK_VIEW";
                /// <summary> Edit an existing project task. </summary>
                public static readonly string PROJ_TASK_EDIT = "PROJ_TASK_EDIT";
                /// <summary> Delete a project task. </summary>
                public static readonly string PROJ_TASK_DELE = "PROJ_TASK_DELE";

                public static class Templates
                {
                    /// <summary> Section for managing project task templates. </summary>
                    public static readonly string PROJ_TASK_TEMP = "PROJ_TASK_TEMP";
                    /// <summary> Create a new task template. </summary>
                    public static readonly string PROJ_TASK_TEMP_NEW = "PROJ_TASK_TEMP_NEW";
                    /// <summary> View details of a task template. </summary>
                    public static readonly string PROJ_TASK_TEMP_VIEW = "PROJ_TASK_TEMP_VIEW";
                    /// <summary> Edit an existing task template. </summary>
                    public static readonly string PROJ_TASK_TEMP_EDIT = "PROJ_TASK_TEMP_EDIT";
                    /// <summary> Delete a task template. </summary>
                    public static readonly string PROJ_TASK_TEMP_DELE = "PROJ_TASK_TEMP_DELE";
                }

                public static class Times
                {
                    /// <summary> Section for managing project task time tracking. </summary>
                    public static readonly string PROJ_TASK_TIME = "PROJ_TASK_TIME";
                    /// <summary> Log new time entry for a project task. </summary>
                    public static readonly string PROJ_TASK_TIME_NEW = "PROJ_TASK_TIME_NEW";
                    /// <summary> View time tracking details for a task. </summary>
                    public static readonly string PROJ_TASK_TIME_VIEW = "PROJ_TASK_TIME_VIEW";
                    /// <summary> Edit a time entry for a project task. </summary>
                    public static readonly string PROJ_TASK_TIME_EDIT = "PROJ_TASK_TIME_EDIT";
                    /// <summary> Delete a time entry from a project task. </summary>
                    public static readonly string PROJ_TASK_TIME_DELE = "PROJ_TASK_TIME_DELE";
                }
            }

        }

        public static class Suppliers
        {
            /// <summary> Section for managing suppliers. </summary>
            public static readonly string SUPL = "SUPL";
            /// <summary> Create a new supplier. </summary>
            public static readonly string SUPL_NEW = "SUPL_NEW";
            /// <summary> View supplier details. </summary>
            public static readonly string SUPL_VIEW = "SUPL_VIEW";
            /// <summary> Edit an existing supplier. </summary>
            public static readonly string SUPL_EDIT = "SUPL_EDIT";
            /// <summary> Delete a supplier. </summary>
            public static readonly string SUPL_DELE = "SUPL_DELE";

            public static class Addresses
            {
                /// <summary> Section for managing supplier addresses. </summary>
                public static readonly string SUPL_ADDR = "SUPL_ADDR";
                /// <summary> Create a new address for a supplier. </summary>
                public static readonly string SUPL_ADDR_NEW = "SUPL_ADDR_NEW";
                /// <summary> View supplier addresses. </summary>
                public static readonly string SUPL_ADDR_VIEW = "SUPL_ADDR_VIEW";
                /// <summary> Edit an existing supplier address. </summary>
                public static readonly string SUPL_ADDR_EDIT = "SUPL_ADDR_EDIT";
                /// <summary> Delete a supplier address. </summary>
                public static readonly string SUPL_ADDR_DELE = "SUPL_ADDR_DELE";
            }

            public static class Documents
            {
                /// <summary> Section for managing supplier documents. </summary>
                public static readonly string SUPL_DOCU = "SUPL_DOCU";
                /// <summary> Upload a new document for a supplier. </summary>
                public static readonly string SUPL_DOCU_NEW = "SUPL_DOCU_NEW";
                /// <summary> View supplier documents. </summary>
                public static readonly string SUPL_DOCU_VIEW = "SUPL_DOCU_VIEW";
                /// <summary> Edit an existing supplier document. </summary>
                public static readonly string SUPL_DOCU_EDIT = "SUPL_DOCU_EDIT";
                /// <summary> Delete a supplier document. </summary>
                public static readonly string SUPL_DOCU_DELE = "SUPL_DOCU_DELE";
            }

            public static class Emails
            {
                /// <summary> Section for managing supplier emails. </summary>
                public static readonly string SUPL_MAIL = "SUPL_MAIL";
                /// <summary> Register a new email for a supplier. </summary>
                public static readonly string SUPL_MAIL_NEW = "SUPL_MAIL_NEW";
                /// <summary> View supplier email details. </summary>
                public static readonly string SUPL_MAIL_VIEW = "SUPL_MAIL_VIEW";
                /// <summary> Edit an existing supplier email. </summary>
                public static readonly string SUPL_MAIL_EDIT = "SUPL_MAIL_EDIT";
                /// <summary> Delete a supplier email. </summary>
                public static readonly string SUPL_MAIL_DELE = "SUPL_MAIL_DELE";
            }

            public static class Invoices
            {
                /// <summary> Section for managing supplier invoices. </summary>
                public static readonly string SUPL_INVO = "SUPL_INVO";
                /// <summary> Create a new invoice for a supplier. </summary>
                public static readonly string SUPL_INVO_NEW = "SUPL_INVO_NEW";
                /// <summary> View supplier invoices. </summary>
                public static readonly string SUPL_INVO_VIEW = "SUPL_INVO_VIEW";
                /// <summary> Edit an existing supplier invoice. </summary>
                public static readonly string SUPL_INVO_EDIT = "SUPL_INVO_EDIT";
                /// <summary> Delete a supplier invoice. </summary>
                public static readonly string SUPL_INVO_DELE = "SUPL_INVO_DELE";
            }

            public static class Notifications
            {
                /// <summary> Section for managing supplier notifications. </summary>
                public static readonly string SUPL_NOTI = "SUPL_NOTI";
                /// <summary> Create a new notification for a supplier. </summary>
                public static readonly string SUPL_NOTI_NEW = "SUPL_NOTI_NEW";
                /// <summary> View supplier notifications. </summary>
                public static readonly string SUPL_NOTI_VIEW = "SUPL_NOTI_VIEW";
                /// <summary> Edit an existing supplier notification. </summary>
                public static readonly string SUPL_NOTI_EDIT = "SUPL_NOTI_EDIT";
                /// <summary> Delete a supplier notification. </summary>
                public static readonly string SUPL_NOTI_DELE = "SUPL_NOTI_DELE";
            }

            public static class Offers
            {
                /// <summary> Section for managing supplier offers. </summary>
                public static readonly string SUPL_OFFE = "SUPL_OFFE";
                /// <summary> Create a new offer for a supplier. </summary>
                public static readonly string SUPL_OFFE_NEW = "SUPL_OFFE_NEW";
                /// <summary> View supplier offers. </summary>
                public static readonly string SUPL_OFFE_VIEW = "SUPL_OFFE_VIEW";
                /// <summary> Edit an existing supplier offer. </summary>
                public static readonly string SUPL_OFFE_EDIT = "SUPL_OFFE_EDIT";
                /// <summary> Delete a supplier offer. </summary>
                public static readonly string SUPL_OFFE_DELE = "SUPL_OFFE_DELE";
            }

            public static class Phones
            {
                /// <summary> Section for managing supplier phone numbers. </summary>
                public static readonly string SUPL_PHON = "SUPL_PHON";
                /// <summary> Register a new phone number for a supplier. </summary>
                public static readonly string SUPL_PHON_NEW = "SUPL_PHON_NEW";
                /// <summary> View supplier phone numbers. </summary>
                public static readonly string SUPL_PHON_VIEW = "SUPL_PHON_VIEW";
                /// <summary> Edit an existing supplier phone number. </summary>
                public static readonly string SUPL_PHON_EDIT = "SUPL_PHON_EDIT";
                /// <summary> Delete a supplier phone number. </summary>
                public static readonly string SUPL_PHON_DELE = "SUPL_PHON_DELE";
            }

            public static class Requests
            {
                /// <summary> Section for managing supplier requests. </summary>
                public static readonly string SUPL_REQU = "SUPL_REQU";
                /// <summary> Create a new request for a supplier. </summary>
                public static readonly string SUPL_REQU_NEW = "SUPL_REQU_NEW";
                /// <summary> View supplier requests. </summary>
                public static readonly string SUPL_REQU_VIEW = "SUPL_REQU_VIEW";
                /// <summary> Edit an existing supplier request. </summary>
                public static readonly string SUPL_REQU_EDIT = "SUPL_REQU_EDIT";
                /// <summary> Delete a supplier request. </summary>
                public static readonly string SUPL_REQU_DELE = "SUPL_REQU_DELE";

                public static class Items
                {
                    /// <summary> Section for managing supplier request items. </summary>
                    public static readonly string SUPL_REQU_ITEM = "SUPL_REQU_ITEM";
                    /// <summary> Add a new item to a supplier request. </summary>
                    public static readonly string SUPL_REQU_ITEM_NEW = "SUPL_REQU_ITEM_NEW";
                    /// <summary> View items in a supplier request. </summary>
                    public static readonly string SUPL_REQU_ITEM_VIEW = "SUPL_REQU_ITEM_VIEW";
                    /// <summary> Edit an item in a supplier request. </summary>
                    public static readonly string SUPL_REQU_ITEM_EDIT = "SUPL_REQU_ITEM_EDIT";
                    /// <summary> Remove an item from a supplier request. </summary>
                    public static readonly string SUPL_REQU_ITEM_DELE = "SUPL_REQU_ITEM_DELE";
                }
            }
        }



        private static readonly Dictionary<string, string> Descriptions = new()
        {
            #region Company
            { Company.COMP, "Company section" },
            { Company.COMP_VIEW, "View company details" },
            { Company.COMP_EDIT, "Edit company details" },
	        #endregion

            #region Customers
            { Customers.CUST, "Customer section" },
            { Customers.CUST_NEW, "Create a new customer" },
            { Customers.CUST_VIEW, "View the customer details" },
            { Customers.CUST_EDIT, "Edit the customer details" },
            { Customers.CUST_DELE, "Delete the customer" },

            // Addresses
            { Customers.Addresses.CUST_ADDR, "Customer addresses section" },
            { Customers.Addresses.CUST_ADDR_NEW, "Create a new address for a customer" },
            { Customers.Addresses.CUST_ADDR_VIEW, "View customer addresses" },
            { Customers.Addresses.CUST_ADDR_EDIT, "Edit customer addresses" },
            { Customers.Addresses.CUST_ADDR_DELE, "Delete customer addresses" },

            // Documents
            { Customers.Documents.CUST_DOCU, "Customer documents section" },
            { Customers.Documents.CUST_DOCU_NEW, "Upload a new document for a customer" },
            { Customers.Documents.CUST_DOCU_VIEW, "View customer documents" },
            { Customers.Documents.CUST_DOCU_EDIT, "Edit customer documents" },
            { Customers.Documents.CUST_DOCU_DELE, "Delete customer documents" },

            // Emails
            { Customers.Emails.CUST_MAIL, "Manage customer emails" },
            { Customers.Emails.CUST_MAIL_NEW, "Create a new email entry for a customer" },
            { Customers.Emails.CUST_MAIL_VIEW, "View customer email details" },
            { Customers.Emails.CUST_MAIL_EDIT, "Edit an existing email entry" },
            { Customers.Emails.CUST_MAIL_DELE, "Delete an email entry" },

            // Feedbacks
            { Customers.Feedbacks.CUST_FEBK, "Manage customer feedback" },
            { Customers.Feedbacks.CUST_FEBK_NEW, "Submit a new feedback entry from a customer" },
            { Customers.Feedbacks.CUST_FEBK_VIEW, "View customer feedback entries" },
            { Customers.Feedbacks.CUST_FEBK_EDIT, "Edit an existing feedback entry" },
            { Customers.Feedbacks.CUST_FEBK_DELE, "Delete a feedback entry" },

            // Invoices
            { Customers.Invoices.CUST_INVO, "Manage customer invoices" },
            { Customers.Invoices.CUST_INVO_NEW, "Create a new invoice for a customer" },
            { Customers.Invoices.CUST_INVO_VIEW, "View invoice details" },
            { Customers.Invoices.CUST_INVO_EDIT, "Edit an existing invoice" },
            { Customers.Invoices.CUST_INVO_DELE, "Delete an invoice" },

            // Notifications
            { Customers.Notifications.CUST_NOTI, "Manage customer notifications" },
            { Customers.Notifications.CUST_NOTI_NEW, "Create a new notification for a customer" },
            { Customers.Notifications.CUST_NOTI_VIEW, "View notification details" },
            { Customers.Notifications.CUST_NOTI_EDIT, "Edit an existing customer notification" },
            { Customers.Notifications.CUST_NOTI_DELE, "Delete a notification" },

            // Orders
            { Customers.Orders.CUST_ORDE, "Manage customer orders" },
            { Customers.Orders.CUST_ORDE_NEW, "Create a new order for a customer" },
            { Customers.Orders.CUST_ORDE_VIEW, "View order details" },
            { Customers.Orders.CUST_ORDE_EDIT, "Edit an existing order" },
            { Customers.Orders.CUST_ORDE_DELE, "Delete an order" },

            // Payments
            { Customers.Payments.CUST_PAYM, "Manage customer payments" },
            { Customers.Payments.CUST_PAYM_NEW, "Register a new payment" },
            { Customers.Payments.CUST_PAYM_VIEW, "View payment details" },
            { Customers.Payments.CUST_PAYM_EDIT, "Edit an existing payment record" },
            { Customers.Payments.CUST_PAYM_DELE, "Delete a payment record" },

            // Phones
            { Customers.Phones.CUST_PHON, "Manage customer phone numbers" },
            { Customers.Phones.CUST_PHON_NEW, "Add a new phone number for a customer" },
            { Customers.Phones.CUST_PHON_VIEW, "View customer phone numbers" },
            { Customers.Phones.CUST_PHON_EDIT, "Edit an existing phone number" },
            { Customers.Phones.CUST_PHON_DELE, "Delete a phone number" },

            // Subscriptions
            { Customers.Subscriptions.CUST_SUBS, "Manage customer subscriptions" },
            { Customers.Subscriptions.CUST_SUBS_NEW, "Create a new subscription for a customer" },
            { Customers.Subscriptions.CUST_SUBS_VIEW, "View subscription details" },
            { Customers.Subscriptions.CUST_SUBS_EDIT, "Edit an existing subscription" },
            { Customers.Subscriptions.CUST_SUBS_DELE, "Cancel a subscription" },

            #endregion

            #region Projects
            // Projects
            { Projects.PROJ, "Manage projects" },
            { Projects.PROJ_NEW, "Create a new project" },
            { Projects.PROJ_VIEW, "View project details" },
            { Projects.PROJ_EDIT, "Edit an existing project" },
            { Projects.PROJ_DELE, "Delete a project" },

            // Project Milestones
            { Projects.Milestones.PROJ_MILE, "Manage project milestones" },
            { Projects.Milestones.PROJ_MILE_NEW, "Create a new milestone within a project" },
            { Projects.Milestones.PROJ_MILE_VIEW, "View details of a project milestone" },
            { Projects.Milestones.PROJ_MILE_EDIT, "Edit an existing project milestone" },
            { Projects.Milestones.PROJ_MILE_DELE, "Delete a project milestone" },

            // Project Resources
            { Projects.Resources.PROJ_RESX, "Manage project resources" },
            { Projects.Resources.PROJ_RESX_NEW, "Add a new resource to a project" },
            { Projects.Resources.PROJ_RESX_VIEW, "View resources assigned to a project" },
            { Projects.Resources.PROJ_RESX_EDIT, "Edit an existing resource in a project" },
            { Projects.Resources.PROJ_RESX_DELE, "Remove a resource from a project" },

            // Project Tasks
            { Projects.ProjectTasks.PROJ_TASK, "Manage project tasks" },
            { Projects.ProjectTasks.PROJ_TASK_NEW, "Create a new task within a project" },
            { Projects.ProjectTasks.PROJ_TASK_VIEW, "View details of a project task" },
            { Projects.ProjectTasks.PROJ_TASK_EDIT, "Edit an existing project task" },
            { Projects.ProjectTasks.PROJ_TASK_DELE, "Delete a project task" },

            // Project Task Templates
            { Projects.ProjectTasks.Templates.PROJ_TASK_TEMP, "Manage project task templates" },
            { Projects.ProjectTasks.Templates.PROJ_TASK_TEMP_NEW, "Create a new task template" },
            { Projects.ProjectTasks.Templates.PROJ_TASK_TEMP_VIEW, "View details of a task template" },
            { Projects.ProjectTasks.Templates.PROJ_TASK_TEMP_EDIT, "Edit an existing task template" },
            { Projects.ProjectTasks.Templates.PROJ_TASK_TEMP_DELE, "Delete a task template" },

            // Project Task Times
            { Projects.ProjectTasks.Times.PROJ_TASK_TIME, "Manage project task time tracking" },
            { Projects.ProjectTasks.Times.PROJ_TASK_TIME_NEW, "Log a new time entry for a project task" },
            { Projects.ProjectTasks.Times.PROJ_TASK_TIME_VIEW, "View time tracking details for a task" },
            { Projects.ProjectTasks.Times.PROJ_TASK_TIME_EDIT, "Edit a time entry for a project task" },
            { Projects.ProjectTasks.Times.PROJ_TASK_TIME_DELE, "Delete a time entry from a project task" },
            #endregion

            #region Suppliers
            { Suppliers.SUPL, "Manage suppliers" },
            { Suppliers.SUPL_NEW, "Create a new supplier" },
            { Suppliers.SUPL_VIEW, "View supplier details" },
            { Suppliers.SUPL_EDIT, "Edit an existing supplier" },
            { Suppliers.SUPL_DELE, "Delete a supplier" },

            // Supplier Addresses
            { Suppliers.Addresses.SUPL_ADDR, "Manage supplier addresses" },
            { Suppliers.Addresses.SUPL_ADDR_NEW, "Create a new address for a supplier" },
            { Suppliers.Addresses.SUPL_ADDR_VIEW, "View supplier addresses" },
            { Suppliers.Addresses.SUPL_ADDR_EDIT, "Edit an existing supplier address" },
            { Suppliers.Addresses.SUPL_ADDR_DELE, "Delete a supplier address" },

            // Supplier Documents
            { Suppliers.Documents.SUPL_DOCU, "Manage supplier documents" },
            { Suppliers.Documents.SUPL_DOCU_NEW, "Upload a new document for a supplier" },
            { Suppliers.Documents.SUPL_DOCU_VIEW, "View supplier documents" },
            { Suppliers.Documents.SUPL_DOCU_EDIT, "Edit an existing supplier document" },
            { Suppliers.Documents.SUPL_DOCU_DELE, "Delete a supplier document" },

            // Supplier Emails
            { Suppliers.Emails.SUPL_MAIL, "Manage supplier emails" },
            { Suppliers.Emails.SUPL_MAIL_NEW, "Register a new email for a supplier" },
            { Suppliers.Emails.SUPL_MAIL_VIEW, "View supplier email details" },
            { Suppliers.Emails.SUPL_MAIL_EDIT, "Edit an existing supplier email" },
            { Suppliers.Emails.SUPL_MAIL_DELE, "Delete a supplier email" },

            // Supplier Invoices
            { Suppliers.Invoices.SUPL_INVO, "Manage supplier invoices" },
            { Suppliers.Invoices.SUPL_INVO_NEW, "Create a new invoice for a supplier" },
            { Suppliers.Invoices.SUPL_INVO_VIEW, "View supplier invoices" },
            { Suppliers.Invoices.SUPL_INVO_EDIT, "Edit an existing supplier invoice" },
            { Suppliers.Invoices.SUPL_INVO_DELE, "Delete a supplier invoice" },

            // Supplier Notifications
            { Suppliers.Notifications.SUPL_NOTI, "Manage supplier notifications" },
            { Suppliers.Notifications.SUPL_NOTI_NEW, "Create a new notification for a supplier" },
            { Suppliers.Notifications.SUPL_NOTI_VIEW, "View supplier notifications" },
            { Suppliers.Notifications.SUPL_NOTI_EDIT, "Edit an existing supplier notification" },
            { Suppliers.Notifications.SUPL_NOTI_DELE, "Delete a supplier notification" },

            // Supplier Offers
            { Suppliers.Offers.SUPL_OFFE, "Manage supplier offers" },
            { Suppliers.Offers.SUPL_OFFE_NEW, "Create a new offer for a supplier" },
            { Suppliers.Offers.SUPL_OFFE_VIEW, "View supplier offers" },
            { Suppliers.Offers.SUPL_OFFE_EDIT, "Edit an existing supplier offer" },
            { Suppliers.Offers.SUPL_OFFE_DELE, "Delete a supplier offer" },

            // Supplier Phones
            { Suppliers.Phones.SUPL_PHON, "Manage supplier phone numbers" },
            { Suppliers.Phones.SUPL_PHON_NEW, "Register a new phone number for a supplier" },
            { Suppliers.Phones.SUPL_PHON_VIEW, "View supplier phone numbers" },
            { Suppliers.Phones.SUPL_PHON_EDIT, "Edit an existing supplier phone number" },
            { Suppliers.Phones.SUPL_PHON_DELE, "Delete a supplier phone number" },

            // Supplier Requests
            { Suppliers.Requests.SUPL_REQU, "Manage supplier requests" },
            { Suppliers.Requests.SUPL_REQU_NEW, "Create a new request for a supplier" },
            { Suppliers.Requests.SUPL_REQU_VIEW, "View supplier requests" },
            { Suppliers.Requests.SUPL_REQU_EDIT, "Edit an existing supplier request" },
            { Suppliers.Requests.SUPL_REQU_DELE, "Delete a supplier request" },

            // Supplier Request Items
            { Suppliers.Requests.Items.SUPL_REQU_ITEM, "Manage supplier request items" },
            { Suppliers.Requests.Items.SUPL_REQU_ITEM_NEW, "Add a new item to a supplier request" },
            { Suppliers.Requests.Items.SUPL_REQU_ITEM_VIEW, "View items in a supplier request" },
            { Suppliers.Requests.Items.SUPL_REQU_ITEM_EDIT, "Edit an item in a supplier request" },
            { Suppliers.Requests.Items.SUPL_REQU_ITEM_DELE, "Remove an item from a supplier request" }
        	#endregion
        };

        public static string GetDescription(string permission)
        {
            return Descriptions.TryGetValue(permission, out var description) ? description : "Unknown Permission";
            //return PermissionsResources.ResourceManager.GetString(permission, CultureInfo.CurrentCulture) ?? "Unknown Permission";

            /*
                COMP	Sezione Azienda
                COMP_VIEW	Visualizza dettagli azienda
                COMP_EDIT	Modifica dettagli azienda
             */
        }
    }
}














