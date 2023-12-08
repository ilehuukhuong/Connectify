import { Component, Input, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { Term } from 'src/app/common/_models/term';
import { AdminService } from 'src/app/common/_services/admin.service';

@Component({
  selector: 'app-term',
  templateUrl: './term.component.html',
  styleUrls: ['./term.component.css']
})
export class TermComponent implements OnInit {
  @Input() listId!: number;
  term: Term[] | undefined;
  newTerm: string = '';
  constructor(private adminService: AdminService, private route: ActivatedRoute, private fb: FormBuilder,
    private toastr: ToastrService,
    private modalService: BsModalService) { }
  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const listId = params['Id'];
      this.listId = listId;
      this.getAllTerm(+listId);
    });
  }
  getAllTerm(listId: number) {
    this.adminService.getAllTerm(listId).subscribe(response => {
      this.term = response.Data.Terms;
      console.log(this.term);
      console.log(this.listId);
    });
  }
  deleteTerm(term: string) {
    if (this.listId !== undefined) {
      this.adminService.deleteTerm(+this.listId, term).subscribe({
        next: (response) => {
          this.term = this.term?.filter(x => x.Term !== term);
        },
        error: (error) => {
        }
      });
    } else {
      console.error('listId is undefined');
    }
  }
  addTerm(newTerm: string) {
    if (this.listId !== undefined && newTerm) { // Kiểm tra xem term không phải là rỗng
      this.adminService.addTerm(+this.listId, newTerm).subscribe({
        next: (response) => {
          this.toastr.success('Term added successfully');
          setTimeout(() => {
            this.getAllTerm(this.listId);
          }, 1500);
          this.newTerm = '';
        },
        error: (error) => {
          this.toastr.error('Failed to add term');
          console.error('Error adding term', error);
        }
      });
    } else {
      if (this.listId === undefined) {
        this.toastr.error('List ID is undefined');
      } else {
        this.toastr.error('Term is required');
      }
    }
  }
  refereshIndex(listId: number) {
    this.adminService.refereshIndex(listId).subscribe(response => {
      this.toastr.success('Apply changes successfully!');
    }, error => {
      this.toastr.error(error);
    });
  }
}